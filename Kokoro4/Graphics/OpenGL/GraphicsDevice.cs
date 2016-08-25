using Kokoro.Graphics.Input.LowLevel;
using Kokoro.Math;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GameWindow = OpenTK.GameWindow;
using FrameEventArgs = OpenTK.FrameEventArgs;
using VSyncMode = OpenTK.VSyncMode;
using Kokoro.StateMachine;
using Kokoro.Engine.Graphics;

namespace Kokoro.Graphics.OpenGL
{
    public enum FaceWinding
    {
        Clockwise = 2304,
        CounterClockwise = 2305
    }

    public class GraphicsDevice
    {
        static VertexArray curVarray;
        static ShaderProgram curProg;
        static Framebuffer curFramebuffer;
        static GameWindow game;
        static List<Tuple<GPUBuffer, int, int>> feedbackBufs;
        static PrimitiveType feedbackPrimitive;

        static FaceWinding winding;
        public static FaceWinding Winding
        {
            get
            {
                return winding;
            }
            set
            {
                winding = value;
                GL.FrontFace((FrontFaceDirection)winding);
            }
        }

        public static Size WindowSize
        {
            get
            {
                return new Size(game.Width, game.Height);
            }
            set
            {
                game.Width = value.Width;
                game.Height = value.Height;
            }
        }

        static Vector4 clearColor;
        public static Vector4 ClearColor
        {
            get
            {
                return clearColor;
            }
            set
            {
                clearColor = value;
                GL.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
            }
        }

        static string gameName;
        public static string Name
        {
            get
            {
                return gameName;
            }
            set
            {
                gameName = value;
                if (Window != null) Window.Title = gameName;
            }
        }
        

        public static StateGroup GameLoop { get; set; }

        public static Action Load { get; set; }
        public static Action<double> Render
        {
            get
            {
                return GameLoop?.Render;
            }
            set
            {
                if (GameLoop != null)
                    GameLoop.Render = value;
            }
        }
        public static Action<double> Update
        {
            get
            {
                return GameLoop?.Update;
            }
            set
            {
                if (GameLoop != null)
                    GameLoop.Update = value;
            }
        }
        public static Action Cleanup { get; set; }
        public static OpenTK.Input.KeyboardDevice Keyboard { get { return game.Keyboard; } }
        public static OpenTK.Input.MouseDevice Mouse { get { return game.Mouse; } }
        public static GameWindow Window
        {
            get
            {
                return game;
            }
        }
        public static int PatchCount
        {
            set
            {
                GL.PatchParameter(PatchParameterInt.PatchVertices, value);
            }
        }

        static bool wframe = false;
        public static bool Wireframe
        {
            set
            {
                wframe = value;
                GL.PolygonMode(MaterialFace.FrontAndBack, value ? PolygonMode.Line : PolygonMode.Fill);
            }
            get
            {
                return wframe;
            }
        }

        static bool aEnabled = false;
        public static bool AlphaEnabled
        {
            get
            {
                return aEnabled;
            }
            set
            {
                aEnabled = value;
                if (aEnabled)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                }
                else
                {
                    GL.Disable(EnableCap.Blend);
                }
            }
        }

        static CullFaceMode cullMode = CullFaceMode.Back;
        public static CullFaceMode CullMode
        {
            get
            {
                return cullMode;
            }
            set
            {
                cullMode = value;
                GL.CullFace(cullMode);
            }
        }

        static bool cullEnabled = false;
        public static bool CullEnabled
        {
            get
            {
                return cullEnabled;
            }
            set
            {
                cullEnabled = value;
                if (cullEnabled)
                    GL.Enable(EnableCap.CullFace);
                else
                    GL.Disable(EnableCap.CullFace);
            }
        }

        static bool depthTestEnabled = false;
        public static bool DepthTestEnabled
        {
            get
            {
                return depthTestEnabled;
            }
            set
            {
                depthTestEnabled = value;
                if (depthTestEnabled)
                {
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthFunc(DepthFunction.Lequal);
                }
                else
                {
                    GL.Disable(EnableCap.DepthTest);
                }
            }
        }

        static bool depthWrite = false;
        public static bool DepthWriteEnabled
        {
            get
            {
                return depthWrite;
            }
            set
            {
                depthWrite = value;
                GL.DepthMask(depthWrite);
            }
        }

        static int workGroupSize = 0;
        public static int ComputeWorkGroupSize
        {
            get
            {
                if (workGroupSize == 0)
                    workGroupSize = GL.GetInteger((GetPName)All.MaxComputeWorkGroupCount);

                return workGroupSize;
            }
        }

        static GraphicsDevice()
        {
            game = new GameWindow(1280, 720);
            game.VSync = VSyncMode.Off;
            game.Resize += Window_Resize;
            game.Load += Game_Load;
            game.RenderFrame += InitRender;
            game.UpdateFrame += Game_UpdateFrame;

            GameLoop = new StateGroup();

            curVarray = null;
            curProg = null;
            feedbackBufs = new List<Tuple<GPUBuffer, int, int>>();
        }


        public static void Run(double ups, double fps)
        {
            game.Title = gameName;
            game.Run(ups, fps);
        }

        static string renderer_name = "";
        public static void SwapBuffers()
        {
#if DEBUG
            if (renderer_name == "")
                renderer_name = GL.GetString(StringName.Renderer);

            game.Title = gameName + $" | {renderer_name} | FPS : {game.RenderFrequency:F2}, UPS : {game.UpdateFrequency:F2}";
#endif
            game.SwapBuffers();
        }

        public static void Exit()
        {
            game.Exit();
        }

        private static void Game_UpdateFrame(object sender, FrameEventArgs e)
        {
            //Update all the input sources
            InputLL.IsFocused(Window.Focused);
            Input.Mouse.Update();
            Input.Keyboard.Update();

            Update?.Invoke(e.Time);
        }

        private static void InitRender(object sender, FrameEventArgs e)
        {
            int major_v = GL.GetInteger(GetPName.MajorVersion);
            int minor_v = GL.GetInteger(GetPName.MinorVersion);
            if (major_v < 4 | (major_v == 4 && minor_v < 5))
            {
                throw new Exception($"Unsupported OpenGL version ({major_v}.{minor_v}), minimum OpenGL 4.5 required.");
            }

            Game_RenderFrame(sender, e);
            game.RenderFrame -= InitRender;
            game.RenderFrame += Game_RenderFrame;
        }

        private static void Game_RenderFrame(object sender, FrameEventArgs e)
        {
            Render?.Invoke(e.Time);
        }

        private static void Game_Load(object sender, EventArgs e)
        {
            curFramebuffer = Framebuffer.Default;
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.TextureCubeMapSeamless);
            GL.NV.DepthRange(-1, 1);
            Load?.Invoke();
        }

        private static void Window_Resize(object sender, EventArgs e)
        {
            GPUStateMachine.SetViewport(0, 0, game.Width, game.Height);
            InputLL.SetWinXY(game.Location.X, game.Location.Y, game.ClientSize.Width, game.ClientSize.Height);
            Framebuffer.RecreateDefaultFramebuffer();
        }

        public static void SetViewport(int x, int y, int width, int height)
        {
            GL.Viewport(x, y, width, height);
        }

        public static void SetShaderProgram(ShaderProgram prog)
        {
            curProg = prog;
        }

        public static void SetVertexArray(VertexArray varray)
        {
            curVarray = varray;
            GL.BindVertexArray(varray.id);
        }

        public static void SetFramebuffer(Framebuffer framebuf)
        {
            curFramebuffer = framebuf;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, curFramebuffer.id);
            SetViewport(0, 0, framebuf.Width, framebuf.Height);
        }

        public static Framebuffer GetFramebuffer()
        {
            return curFramebuffer;
        }

        public static void SetFeedbackBuffer(int slot, GPUBuffer buf)
        {
            SetFeedbackBuffer(slot, buf, 0, buf.size);
        }

        public static void SetFeedbackBuffer(int slot, GPUBuffer buf, int offset, int size)
        {
            while (feedbackBufs.Count <= slot)
                feedbackBufs.Add(null);

            feedbackBufs[slot] = new Tuple<GPUBuffer, int, int>(buf, offset, size);
        }


        public static void SetFeedbackPrimitive(PrimitiveType type)
        {
            if (feedbackPrimitive == PrimitiveType.Points || feedbackPrimitive == PrimitiveType.Lines || feedbackPrimitive == PrimitiveType.Triangles) feedbackPrimitive = type;
            else throw new Exception();
        }

        public static void DispatchComputeJob(ShaderProgram prog, int x, int y, int z)
        {
            GL.UseProgram(prog.prog.id);
            GL.DispatchCompute(x, y, z);
        }

        public static void Draw(PrimitiveType type, int first, int count, bool indexed)
        {
            if (count == 0) return;

            if (curVarray == null) return;
            if (curProg == null) return;
            if (curFramebuffer == null) return;


            for (int i = 0; i < feedbackBufs.Count; i++) GPUStateMachine.BindBuffer(BufferTarget.TransformFeedbackBuffer, feedbackBufs[i].Item1.id, i, (IntPtr)feedbackBufs[i].Item2, (IntPtr)feedbackBufs[i].Item3);

            if (feedbackBufs.Count > 0) GL.BeginTransformFeedback((TransformFeedbackPrimitiveType)feedbackPrimitive);

            curProg.Set(nameof(WindowSize), new Vector2(WindowSize.Width, WindowSize.Height));

            GL.UseProgram(curProg.prog.id);

            if (indexed) GL.DrawElements(type, count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            else GL.DrawArrays(type, first, count);

            if (feedbackBufs.Count > 0) GL.EndTransformFeedback();

            for (int i = 0; i < feedbackBufs.Count; i++) GPUStateMachine.UnbindBuffer(BufferTarget.TransformFeedbackBuffer, i);

            feedbackBufs.Clear();
        }

        public static void Clear()
        {
            // render graphics
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public static void SaveTexture(Texture t, string file)
        {
#if DEBUG
            Bitmap bmp = new Bitmap(t.Width, t.Height);
            System.Drawing.Imaging.BitmapData bmpData;

            bmpData = bmp.LockBits(new Rectangle(0, 0, t.Width, t.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GPUStateMachine.BindTexture(0, (OpenTK.Graphics.OpenGL.TextureTarget)t.texTarget, t.id);
            GL.GetTexImage((OpenTK.Graphics.OpenGL.TextureTarget)t.texTarget, 0, (OpenTK.Graphics.OpenGL.PixelFormat)Engine.Graphics.PixelFormat.Bgra, (OpenTK.Graphics.OpenGL.PixelType)Engine.Graphics.PixelType.UnsignedInt8888Reversed, bmpData.Scan0);
            GPUStateMachine.UnbindTexture(0, (OpenTK.Graphics.OpenGL.TextureTarget)t.texTarget);
            bmp.UnlockBits(bmpData);

            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
            bmp.Save(file);
            bmp.Dispose();
#endif
        }

    }
}
