﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;
using Vulkan.Windows;

namespace Kokoro.Graphics.Vulkan
{
    public class GraphicsDevice
    {
        static GraphicsDevice()
        {

        }

        public static string Name { get; set; }
        public static uint MajorVersion { get; set; }
        public static uint MinorVersion { get; set; }
        public static uint PatchVersion { get; set; }

        private static GameWindow Window { get; set; }
        private static Instance inst;
        private static PhysicalDevice PhysDevice;
        private static Device LogicalDevice;
        private static List<Queue> Queues;
        private static SurfaceKhr DrawSurface;
        private static SwapchainKhr Swapchain;
        private static Image[] SwapchainImages;
        private static PhysicalDeviceFeatures Features = new PhysicalDeviceFeatures()
        {
            GeometryShader = true,
            MultiDrawIndirect = true,
            TessellationShader = true,
            FillModeNonSolid = true,
            SparseBinding = true,
            SparseResidencyImage2D = true,
            SparseResidencyBuffer = true,
            DrawIndirectFirstInstance = true,
            FullDrawIndexUint32 = true,
            ShaderResourceResidency = true,
        };

#if DEBUG
        private static Bool32 debugReportCallback(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, ulong objectHandle, IntPtr location, int messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData)
        {
            return false;
        }
#endif

        internal static ShaderModule CreateShaderModule(ShaderModuleCreateInfo info)
        {
            return LogicalDevice.CreateShaderModule(info);
        }

        public static void Run(double ups, double fps)
        {
            var extensions = new string[]
            {
                "VK_KHR_surface",
                "VK_KHR_win32_surface",
#if DEBUG
                "VK_EXT_debug_report",
#endif
            };

            var device_extensions = new string[]
            {
                "VK_KHR_swapchain"
            };

            var layers = new string[]
            {
#if DEBUG
                "VK_LAYER_LUNARG_standard_validation",
#endif
            };

            ApplicationInfo appInfo = new ApplicationInfo()
            {
                ApiVersion = global::Vulkan.Version.Make(1, 0, 0),
                EngineName = typeof(GraphicsDevice).Assembly.GetName().Name,
                ApplicationName = Name,
                ApplicationVersion = global::Vulkan.Version.Make(MajorVersion, MinorVersion, PatchVersion),
                EngineVersion = global::Vulkan.Version.Make((uint)typeof(GraphicsDevice).Assembly.GetName().Version.Major, (uint)typeof(GraphicsDevice).Assembly.GetName().Version.Minor, (uint)typeof(GraphicsDevice).Assembly.GetName().Version.MinorRevision)
            };

            inst = new Instance(new InstanceCreateInfo()
            {
                ApplicationInfo = appInfo,
                EnabledExtensionNames = extensions,
                EnabledExtensionCount = (uint)extensions.Length,
                EnabledLayerNames = layers,
                EnabledLayerCount = (uint)layers.Length
            });

#if DEBUG
            inst.EnableDebug(debugReportCallback);
#endif

            //Create a render surface
            Window = new GameWindow();
            Window.CreateControl();

            Win32SurfaceCreateInfoKhr surf_create_info = new Win32SurfaceCreateInfoKhr()
            {
                Hwnd = Window.Handle,
                Hinstance = System.Runtime.InteropServices.Marshal.GetHINSTANCE(typeof(GraphicsDevice).Module)
            };

            DrawSurface = inst.CreateWin32SurfaceKHR(surf_create_info);

            //Get the necessary Queues
            int gfx_queue_index = -1;
            int compute_queue_index = -1;
            int transfer_queue_index = -1;
            int present_queue_index = -1;

            Func<PhysicalDevice, bool> appropriateDevice = (device) =>
            {
                //TODO check device to ensure it supports the required device extensions

                if (device.GetFeatures().DrawIndirectFirstInstance && device.GetFeatures().SparseResidencyImage2D && device.GetFeatures().SparseResidencyImage2D)
                {
                    int queue_score = 0;

                    var queue_props = device.GetQueueFamilyProperties();
                    for (int i = 0; i < queue_props.Length && queue_score < 3; i++)
                    {
                        if (device.GetSurfaceSupportKHR((uint)i, DrawSurface) && (present_queue_index != gfx_queue_index || present_queue_index == -1))
                        {
                            present_queue_index = i;
                        }

                        //Check to see if the required queues are available
                        if ((queue_props[i].QueueFlags & (QueueFlags.Graphics | QueueFlags.SparseBinding)) == (QueueFlags.Graphics | QueueFlags.SparseBinding))
                        {
                            queue_score++;
                            gfx_queue_index = i;
                            continue;
                        }
                        if ((queue_props[i].QueueFlags & QueueFlags.Compute) == QueueFlags.Compute)
                        {
                            queue_score++;
                            compute_queue_index = i;
                            if (transfer_queue_index == -1) transfer_queue_index = i;
                            continue;
                        }
                        //Prefer to have a separate transfer queue, but isn't necessary, we can just use the compute queue
                        if ((queue_props[i].QueueFlags & QueueFlags.Transfer) == QueueFlags.Transfer)
                        {
                            queue_score++;
                            transfer_queue_index = i;
                            continue;
                        }
                    }

                    if (queue_score >= 2) return true;
                }

                return false;
            };

            //Enumerate and select an appropriate physical device
            var devices = inst.EnumeratePhysicalDevices();
            if (devices == null || devices.Length == 0)
                throw new Exception("This system does not have Vulkan support installed");
            for (int i = 0; i < devices.Length; i++)
            {
                if (appropriateDevice(devices[i]))
                {
                    PhysDevice = devices[i];
                    break;
                }
            }
            if (PhysDevice == null) throw new Exception("GPU is not supported");


            //Create a logical device
            List<DeviceQueueCreateInfo> dev_q_create_info = new List<DeviceQueueCreateInfo>();

            dev_q_create_info.Add(new DeviceQueueCreateInfo()
            {
                QueueFamilyIndex = (uint)gfx_queue_index,
                QueuePriorities = new float[] { 1.0f },
                QueueCount = 1
            });

            dev_q_create_info.Add(new DeviceQueueCreateInfo()
            {
                QueueFamilyIndex = (uint)compute_queue_index,
                QueuePriorities = new float[] { 1.0f },
                QueueCount = 1
            });

            if (transfer_queue_index != compute_queue_index && transfer_queue_index != gfx_queue_index && transfer_queue_index != present_queue_index)
            {
                dev_q_create_info.Add(new DeviceQueueCreateInfo()
                {
                    QueueFamilyIndex = (uint)transfer_queue_index,
                    QueuePriorities = new float[] { 1.0f },
                    QueueCount = 1
                });
            }

            if (present_queue_index != transfer_queue_index && present_queue_index != compute_queue_index && present_queue_index != gfx_queue_index)
            {
                dev_q_create_info.Add(new DeviceQueueCreateInfo()
                {
                    QueueFamilyIndex = (uint)present_queue_index,
                    QueuePriorities = new float[] { 1.0f },
                    QueueCount = 1
                });
            }

            DeviceCreateInfo dev_create_info = new DeviceCreateInfo()
            {
                QueueCreateInfos = dev_q_create_info.ToArray(),
                QueueCreateInfoCount = (uint)dev_q_create_info.Count,
                EnabledFeatures = Features,
                EnabledExtensionNames = device_extensions,
                EnabledExtensionCount = (uint)device_extensions.Length,
                EnabledLayerNames = layers,
                EnabledLayerCount = (uint)layers.Length
            };

            LogicalDevice = PhysDevice.CreateDevice(dev_create_info);

            Queues = new List<Queue>();
            for (int i = 0; i < dev_q_create_info.Count; i++)
            {
                Queues.Add(LogicalDevice.GetQueue(dev_q_create_info[i].QueueFamilyIndex, 0));
            }

            //Create swapchain
            SurfaceCapabilitiesKhr surface_cap_khr = PhysDevice.GetSurfaceCapabilitiesKHR(DrawSurface);
            SurfaceFormatKhr[] surface_formats = PhysDevice.GetSurfaceFormatsKHR(DrawSurface);
            PresentModeKhr[] present_modes = PhysDevice.GetSurfacePresentModesKHR(DrawSurface);

            SwapchainCreateInfoKhr swapchain_create_info = new SwapchainCreateInfoKhr()
            {

            };

            Swapchain = LogicalDevice.CreateSwapchainKHR(swapchain_create_info);
            SwapchainImages = LogicalDevice.GetSwapchainImagesKHR(Swapchain);

            
        }


    }
}
