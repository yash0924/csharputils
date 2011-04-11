﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSharpPspEmulator.Core
{
	unsafe public class Memory
	{
        sealed class SegmentsClass
        {
            sealed class Segment
            {
                String Name;
                byte[] Memory;
                uint Start;
                uint End;

                public Segment(String Name, uint Start, uint Length)
                {
                    this.Name = Name;
                    this.Start = Start;
                    this.End = Start + Length;
                    this.Memory = new byte[Length];
                }

                public bool IsAddressInside(uint Address)
                {
                    return (Address >= this.Start && Address < this.End);
                }

                public byte* GetPointer(uint Address)
                {
                    if (IsAddressInside(Address))
                    {
                        fixed (byte* MemoryPtr = &Memory[Address - this.Start])
                        {
                            return MemoryPtr;
                        }
                    }
                    return null;
                }
            }

            readonly private Segment ScratchPad;
            readonly private Segment FrameBuffer;
            readonly private Segment MainMemory;
            readonly private Segment[] Segments;

            public SegmentsClass()
            {
                Segments = new Segment[3];
                Segments[0] = ScratchPad = new Segment("ScratchPad", 0x00010000, 0x00004000);
                Segments[1] = FrameBuffer = new Segment("FrameBuffer", 0x04000000, 0x00200000);
                Segments[2] = MainMemory = new Segment("MainMemory", 0x08000000, 0x02000000);
            }

            public unsafe byte* GetPointer(uint Address)
            {
                foreach (var Segment in Segments)
                {
                    byte *Pointer = Segment.GetPointer(Address);
                    if (Pointer != null) return Pointer;
                }
                throw(new Exception(String.Format("Invalid Address {0,8:X}", Address)));
                //return null;
            }
        }

        SegmentsClass Segments = new SegmentsClass();

        public unsafe byte* GetPointer(uint Address)
		{
            return Segments.GetPointer(Address);
		}

        public unsafe IntPtr GetIntPtr(uint Address)
        {
            return (IntPtr)GetPointer(Address);
        }

        public byte ReadUnsigned8(uint Address) { return *((byte*)GetPointer(Address)); }
        public ushort ReadUnsigned16(uint Address) { return *((ushort*)GetPointer(Address)); }
        public uint ReadUnsigned32(uint Address) { return *((uint *)GetPointer(Address)); }
        public ulong ReadUnsigned64(uint Address) { return *((ulong*)GetPointer(Address)); }

        public void WriteUnsigned8(uint Address, byte Value) { *((byte*)GetPointer(Address)) = Value; }
        public void WriteUnsigned16(uint Address, ushort Value) { *((ushort*)GetPointer(Address)) = Value; }
        public void WriteUnsigned32(uint Address, uint Value) { *((uint*)GetPointer(Address)) = Value; }
        public void WriteUnsigned64(uint Address, ulong Value) {
            Marshal.WriteInt64((IntPtr)GetPointer(Address), (long)Value);
        }

        public void WriteBytes(uint Address, byte[] Data, uint Position, int Length)
        {
            var Pointer = (IntPtr)GetPointer(Address);
            for (int n = 0; n < Length; n++)
            {
                Marshal.WriteByte(Pointer, n, Data[Position + n]);
            }
        }
    }
}
