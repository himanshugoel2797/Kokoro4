using Accord.Imaging;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Optimization.Losses;
using Accord.Neuro;
using Accord.Neuro.Learning;
using Accord.Statistics.Kernels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvNetSharp.Volume;
using ConvNetSharp.Volume.Single;
using System.IO;

#if DEEP_NN
using ConvNetSharp.Core;
using ConvNetSharp.Core.Fluent;
using ConvNetSharp.Core.Training;
using Volume = ConvNetSharp.Volume.Volume<float>;
#endif

namespace KinectGestureInterface.Kinect
{
    public class ImageMatching
    {
#if !DEEP_NN
#if !DIRECT_ANN
        public Bitmap[] CompareTargets { get; private set; }
#else
        public double[][] CompareTargets { get; private set; }
#endif
        public BagOfVisualWords Words;
#endif

#if ANN
        public ActivationNetwork NeuralNet;
#endif

#if DIRECT_ANN
        public ActivationNetwork NeuralNet;
#endif

#if SVM
        public MultilabelSupportVectorMachine<Gaussian> Svm;
#endif
#if DEEP_NN
        private INet<float> Net;
        private SgdTrainer<float> Trainer;
#endif

        private string[] Gestures = new string[]
        {
            "closed_hand",
            "one_fingers",
            "two_fingers",
            "six",
            "four_fingers",
            "open_hand",
            "three_fingers",
            "seven",
            "Y",
            "nine",
        };

#if DEEP_NN
        private void Test(Volume x, int[] labels, bool forward = true)
        {
            if (forward)
                this.Net.Forward(x);
            var prediction = this.Net.GetPrediction();
        }
#endif

        public ImageMatching()
        {
            int sample_cnt = 1024;

            int[][] labels_d = new int[sample_cnt][];
#if !DEEP_NN
            int feature_cnt = 40;
#if !DIRECT_ANN
            CompareTargets = new Bitmap[Gestures.Length * sample_cnt];
#else
            CompareTargets = new double[Gestures.Length * sample_cnt][];
#endif

            double[][] nn_labels = new double[Gestures.Length * sample_cnt][];
#else
            var vols_Shape = new Shape(128, 128, 1, Gestures.Length);
            var labels_Shape = new Shape(1, 1, Gestures.Length, Gestures.Length);

            var vols = new Volume[sample_cnt];
            var labels = new Volume[sample_cnt];
#endif

            if (!File.Exists("network_1.xml"))
            {
                int idx = 0;
                for (int i = 0; i < Gestures.Length; i++)
                {
                    for (int j = 0; j < sample_cnt; j++)
                    {
#if !DEEP_NN
#if !DIRECT_ANN
                    CompareTargets[idx] = LoadImage(Gestures[i] + j.ToString() + ".png");
#else
                        CompareTargets[idx] = LoadImageToArray(Gestures[i] + j.ToString() + ".png");
#endif
                        nn_labels[idx] = new double[Gestures.Length];
                        nn_labels[idx][i] = 1;
                        idx++;
#else
                    if (vols[j] == null) vols[j] = BuilderInstance.Volume.From(new float[Gestures.Length * 128 * 128], vols_Shape);
                    if (labels[j] == null) labels[j] = BuilderInstance.Volume.From(new float[Gestures.Length * Gestures.Length], labels_Shape);

                    if (labels_d[j] == null) labels_d[j] = new int[Gestures.Length];
                    labels_d[j][i] = i;

                    LoadImageToVolume(Gestures[i] + j.ToString() + ".png", vols[j], i);
                    labels[j].Set(0, 0, i, idx, 1);
#endif
                    }
                }

                Random rnd = new Random(0);
                CompareTargets = CompareTargets.OrderBy(x => rnd.Next()).ToArray();

                rnd = new Random(0);
                nn_labels = nn_labels.OrderBy(x => rnd.Next()).ToArray();
            }
#if DEEP_NN
            this.Net = FluentNet<float>.Create(128, 128, 1)
                .Conv(50, 50, 8).Stride(1).Pad(2)
                .Relu()
                .Pool(20, 20).Stride(2)
                .Conv(5, 5, 16).Stride(1).Pad(2)
                .Relu()
                .Pool(Gestures.Length, Gestures.Length).Stride(3)
                .FullyConn(Gestures.Length)
                .Softmax(Gestures.Length)
                .Build();

            this.Trainer = new SgdTrainer<float>(this.Net)
            {
                LearningRate = 0.01f,
                BatchSize = 20,
                L2Decay = 0.001f,
                Momentum = 0.9f
            };

            for (int i = 0; i < sample_cnt; i++)
            {
                Trainer.Train(vols[i], labels[i]);
                Test(vols[i], labels_d[i]);
            }
#endif

#if !DEEP_NN
#if !DIRECT_ANN
            Accord.Math.Random.Generator.Seed = 0;

            Words = BagOfVisualWords.Create(feature_cnt);
            Words.ParallelOptions.MaxDegreeOfParallelism = 1;

            //var descriptors = Words.Detector.ProcessImage(CompareTargets[0]);
            //var features_0 = descriptors.Select(d => d.Descriptor).ToArray();

            Words.Learn(CompareTargets);
            var features = Words.Transform(CompareTargets);
#endif
#endif

#if SVM
            var teacher = new MultilabelSupportVectorLearning<Gaussian>()
            {
                // using LIBLINEAR's L2-loss SVC dual for each SVM
                Learner = (p) => new SequentialMinimalOptimization<Gaussian>
                {
                    UseKernelEstimation = true,
                }
            };

            Svm = teacher.Learn(features, labels);
            var output = Svm.Decide(features);

            // Create the multi-class learning algorithm for the machine
            var calibration = new MultilabelSupportVectorLearning<Gaussian>()
            {
                Model = Svm, // We will start with an existing machine

                // Configure the learning algorithm to use SMO to train the
                //  underlying SVMs in each of the binary class subproblems.
                Learner = (param) => new ProbabilisticOutputCalibration<Gaussian>()
                {
                    Model = param.Model // Start with an existing machine
                }
            };

            // Configure parallel execution options
            calibration.ParallelOptions.MaxDegreeOfParallelism = 1;

            // Learn a machine
            calibration.Learn(features, labels);
#elif ANN
            NeuralNet = new ActivationNetwork(new SigmoidFunction(), feature_cnt, 30, 15, Gestures.Length);

            new NguyenWidrow(NeuralNet).Randomize();

            var NN_teacher = new LevenbergMarquardtLearning(NeuralNet);

            double error = Double.PositiveInfinity;
            for (int i = 0; i < 40; i++)
            {
                error = NN_teacher.RunEpoch(features, nn_labels);
                Console.WriteLine($"Error {i} : {error}");
            }
#elif DIRECT_ANN

            if (!File.Exists("network_1.xml"))
            {
                NeuralNet = new ActivationNetwork(new SigmoidFunction(), 128 * 128, 1024, 256, 64, 32, Gestures.Length);

                new NguyenWidrow(NeuralNet).Randomize();

                var NN_teacher = new BackPropagationLearning(NeuralNet); //new LevenbergMarquardtLearning(NeuralNet);
                double error = Double.PositiveInfinity;
                for (int i = 0; i < 5000; i++)
                {
                    error = NN_teacher.RunEpoch(CompareTargets, nn_labels);
                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Error {i} : {error}");

                    if (error < CompareTargets.Length / 100)
                        break;
                }
                NeuralNet.Save("network_1.xml");
            }
            else
                NeuralNet = (ActivationNetwork)Network.Load("network_1.xml");
#endif

            // Compute the error between the expected and predicted labels
            //double error = new ZeroOneLoss(labels).Loss(output);
            //Console.WriteLine($"Error: {error}");
        }

        private static Bitmap LoadImage(string img)
        {
            Bitmap bmp = Accord.Imaging.Image.Clone((Bitmap)System.Drawing.Image.FromFile(img));
            return bmp;
        }

        private static double[] LoadImageToArray(string img)
        {
            Bitmap bmp = ((Bitmap)System.Drawing.Image.FromFile(img));

            double[] f = new double[bmp.Width * bmp.Height];

            int i = 0;
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                    f[i++] = bmp.GetPixel(x, y).R / 255.0f;

            return f;
        }

        private static void LoadImageToVolume(string img, Volume dst, int layer)
        {
            Bitmap bmp = ((Bitmap)System.Drawing.Image.FromFile(img));

            int i = 0;
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                    dst.Set(x, y, 0, layer, bmp.GetPixel(x, y).R / 255.0f);

        }

        public int BestMatch(Bitmap src)
        {
#if !DEEP_NN
#if !DIRECT_ANN
            var features = Words.Transform(src);
#endif
#endif
#if SVM
            var res = Svm.Decide(features);
            var probs = Svm.Probabilities(features);
            Console.Write("{");
            for (int i = 0; i < res.Length; i++)
                Console.Write($"{probs[i]},");

            Console.Write("\b}\n");

            for (int i = 0; i < res.Length; i++)
                if (res[i])
                    return i;
#elif ANN
            double[] output = NeuralNet.Compute(features);
            
            for(int i = 0; i < output.Length; i++)
            {
                if (output[i] == output.Max() && output[i] >= 0.6f)
                {
                    return i;
                }
            }
#elif DIRECT_ANN

            double[] input = new double[128 * 128];
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    input[y * src.Width + x] = src.GetPixel(x, y).R / 255.0f;

            double[] output = NeuralNet.Compute(input);

            for (int i = 0; i < output.Length; i++)
            {
                if (output[i] == output.Max() && output[i] >= 0.6f)
                {
                    return i;
                }
            }
#endif
            return -1;
        }
    }
}
