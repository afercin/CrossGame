//using Cloo;
//using OpenCL;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace Cross_Game.DataManipulation
//{
//    class GPUAceleration
//    {
//        private EasyCL cl;

//        private ComputeCommandQueue queue;
//        private ComputeContext context;
//        private ComputeKernel kernel;

//        public GPUAceleration()
//        {
//            cl = new EasyCL()
//            {
//                Accelerator = AcceleratorDevice.GPU
//            };
//            cl.LoadKernel(ArrayCopy);

//            context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(ComputePlatform.Platforms[0]), null, IntPtr.Zero);
//            queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
//            var program = new ComputeProgram(context, ArrayCopy);            
//            program.Build(null, null, null, IntPtr.Zero);

//            kernel = program.CreateKernel("ArrayCopy");
//        }

//        public void GPUCopy2(byte[] origin, int originOffset, byte[] dest, int destOffset, int lenght)
//        {
//            cl.Invoke("ArrayCopy", 0, 5, new object[] { origin, originOffset, dest, destOffset, lenght });
//        }
//        public void GPUCopy(byte[] origin, int originOffset, byte[] dest, int destOffset, int lenght)
//        {
            
//            ComputeBuffer<byte> originBuffer, destBuffer;
//            GCHandle arrCHandle;

//            originBuffer = new ComputeBuffer<byte>(context,
//                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, origin);            
//            destBuffer = new ComputeBuffer<byte>(context,
//                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, dest);

//            kernel.SetMemoryArgument(0, originBuffer);
//            kernel.SetValueArgument(1, originOffset);
//            kernel.SetMemoryArgument(2, destBuffer);
//            kernel.SetValueArgument(3, destOffset);

//            queue.Execute(kernel, new long[] { 0 }, new long[] { lenght }, null, new ComputeEventList());
//            queue.Finish();

//            arrCHandle = GCHandle.Alloc(destOffset, GCHandleType.Pinned);
//            queue.Read(destBuffer, true, destOffset, lenght, arrCHandle.AddrOfPinnedObject(), null);
//        }


//        private string ArrayCopy
//        {
//            get
//            {
//                return @"
//                        kernel void ArrayCopy(global uchar* bufferIn, int inOffset, global uchar* bufferOut, int outOffset) 
//                        {
//                              int index = get_global_id(0);
//                                  bufferOut[index + outOffset] = bufferIn[index + inOffset];
//                        }";
//            }
//        }
//    }
//}
