﻿/*
 Copyright (c) 2012-2013 Clint Banzhaf
 This file is part of "Meridian59 .NET".

 "Meridian59 .NET" is free software: 
 You can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, 
 either version 3 of the License, or (at your option) any later version.

 "Meridian59 .NET" is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with "Meridian59 .NET".
 If not, see http://www.gnu.org/licenses/.
*/

using Meridian59.Common;
using Meridian59.Common.Constants;
using Meridian59.Common.Interfaces;
using System;

namespace Meridian59.Files.ROO
{
    [Serializable]
    public abstract class RooBSPItem : IByteSerializableFast, IRooIndicesResolvable
    {
        /// <summary>
        /// Different types of nodes in a tree.
        /// Node = Has at least one child.
        /// Leaf = Has no children
        /// </summary>
        public enum NodeType : byte
        {
            Node = 0x01, Leaf=0x02
        }

        #region IByteSerializable
        public virtual int ByteLength
        {
            get { return TypeSizes.BYTE + TypeSizes.INT + TypeSizes.INT + TypeSizes.INT + TypeSizes.INT; }
        }

        public virtual int WriteTo(byte[] Buffer, int StartIndex = 0)
        {
            int cursor = StartIndex;

            Buffer[cursor] = (byte)Type;
            cursor++;

            Array.Copy(BitConverter.GetBytes(X1), 0, Buffer, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;

            Array.Copy(BitConverter.GetBytes(Y1), 0, Buffer, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;

            Array.Copy(BitConverter.GetBytes(X2), 0, Buffer, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;

            Array.Copy(BitConverter.GetBytes(Y2), 0, Buffer, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;
            
            return cursor - StartIndex;
        }

        public virtual unsafe void WriteTo(ref byte* Buffer)
        {
            Buffer[0] = (byte)Type;
            Buffer++;

            *((int*)Buffer) = X1;
            Buffer += TypeSizes.INT;

            *((int*)Buffer) = Y1;
            Buffer += TypeSizes.INT;

            *((int*)Buffer) = X2;
            Buffer += TypeSizes.INT;

            *((int*)Buffer) = Y2;
            Buffer += TypeSizes.INT;
        }

        public virtual int ReadFrom(byte[] Buffer, int StartIndex = 0)
        {
            int cursor = StartIndex;

            // no need to read type
            cursor++;

            X1 = BitConverter.ToInt32(Buffer, cursor);
            cursor += TypeSizes.INT;

            Y1 = BitConverter.ToInt32(Buffer, cursor);
            cursor += TypeSizes.INT;

            X2 = BitConverter.ToInt32(Buffer, cursor);
            cursor += TypeSizes.INT;

            Y2 = BitConverter.ToInt32(Buffer, cursor);
            cursor += TypeSizes.INT;
            
            return cursor - StartIndex;
        }

        public virtual unsafe void ReadFrom(ref byte* Buffer)
        {
            // no need to read type
            Buffer++;

            X1 = *((int*)Buffer);
            Buffer += TypeSizes.INT;

            Y1 = *((int*)Buffer);
            Buffer += TypeSizes.INT;

            X2 = *((int*)Buffer);
            Buffer += TypeSizes.INT;

            Y2 = *((int*)Buffer);
            Buffer += TypeSizes.INT;
        }

        public byte[] Bytes
        {
            get
            {
                byte[] returnValue = new byte[ByteLength];
                WriteTo(returnValue);
                return returnValue;
            }
            set
            {
                ReadFrom(value);
            }
        }
        #endregion

        protected BoundingBox2D boundingBox;

        /// <summary>
        /// Abstract. Type to set by deriving subclasses.
        /// </summary>
        public abstract NodeType Type { get; }

        /// <summary>
        /// A 2D boundingbox of this BSP node (splitter or leaf).
        /// </summary>
        public BoundingBox2D BoundingBox { get { return boundingBox; } set { boundingBox = value; } }

        /// <summary>
        /// BoundingBox minimum X of this node (or leaf).
        /// </summary>
        public int X1 { get { return (int)boundingBox.Min.X; } set { boundingBox.Min.X = value; } }

        /// <summary>
        /// BoundingBox minimum Y of this node (or leaf).
        /// </summary>
        public int Y1 { get { return (int)boundingBox.Min.Y; } set { boundingBox.Min.Y = value; } }

        /// <summary>
        /// BoundingBox maximum X of this node (or leaf).
        /// </summary>
        public int X2 { get { return (int)boundingBox.Max.X; } set { boundingBox.Max.X = value; } }

        /// <summary>
        /// BoundingBox maximum Y of this node (or leaf).
        /// </summary>
        public int Y2 { get { return (int)boundingBox.Max.Y; } set { boundingBox.Max.Y = value; } }

        /// <summary>
        /// Constructor by values
        /// </summary>
        public RooBSPItem()
        {
        }

        /// <summary>
        /// Constructor by managed parser
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="StartIndex"></param>
        public RooBSPItem(byte[] Buffer, int StartIndex = 0)
        {
            ReadFrom(Buffer, StartIndex);
        }

        /// <summary>
        /// Constructor by native parser
        /// </summary>
        /// <param name="Buffer"></param>
        public unsafe RooBSPItem(ref byte* Buffer)
        {
            ReadFrom(ref Buffer);
        }

        /// <summary>
        /// Abstract. Must be implemented.
        /// </summary>
        /// <param name="RooFile"></param>
        public abstract void ResolveIndices(RooFile RooFile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="StartIndex"></param>
        /// <returns></returns>
        public static RooBSPItem ExtractBSPItem(byte[] Buffer, int StartIndex)
        {          
            switch ((NodeType)Buffer[StartIndex])
            {
                case NodeType.Node:
                    return new RooPartitionLine(Buffer, StartIndex);
                    
                case NodeType.Leaf:
                    return new RooSubSector(Buffer, StartIndex);

                default:
                    throw new Exception("Unsupported BSPItem type: "+Buffer[StartIndex]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Buffer"></param>
        /// <returns></returns>
        public static unsafe RooBSPItem ExtractBSPItem(ref byte* Buffer)
        {
            switch ((NodeType)Buffer[0])
            {
                case NodeType.Node:
                    return new RooPartitionLine(ref Buffer);

                case NodeType.Leaf:
                    return new RooSubSector(ref Buffer);

                default:
                    throw new Exception("Unsupported BSPItem type: " + Buffer[0]);
            }
        }
    }
}
