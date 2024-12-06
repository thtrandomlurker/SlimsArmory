using RaCLib.IO;
using System.Buffers.Binary;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace RaCLib.Armor
{
    public enum ArmorTextureFormat : byte
    {
        BC1 = 0x86,
        BC2 = 0x87,
        BC3 = 0x88
    }
    public class ArmorTexturedSubmesh
    {
        public ushort[]? Indices;
        public int IndexCount;
        public int TextureIndex;
        public int Flags;
    }
    public class ArmorReflectiveSubmesh
    {
        public ushort[]? Indices;
        public int IndexCount;
        public int ReflectionMode;  // Is this correct? I honestly don't know...
        public int Flags;
    }

    public class MipMap
    {
        public short Width;
        public short Height;
        public byte[]? MipData;
    }

    public class ArmorTexture
    {
        // int textureOffset;
        public List<MipMap> MipMaps;
        public byte Unk04;
        public byte MipMapCount;
        public ArmorTextureFormat Format;
        public byte Unk07;
        public byte Unk08;
        public byte Unk09;
        public byte Unk0A;
        public byte Unk0B;
        public byte Unk0C;
        public byte Unk0D;
        public byte Unk0E;
        public byte Unk0F;
        public int GTFFlags;  // I've seen this value pop up in some games before
        public byte Unk14;
        public byte Unk15;
        public byte Unk16;
        public byte Unk17;
        public short Width;
        public short Height;
        public short Unk1C;
        public short Unk1E;
        public short Unk20;  // this value can be found int the vita version, flipped, 0x00FF usually
        public short Unk22;

        public void Read(EndianBinaryReader reader, Stream texStream)
        {
            int texDataOffset = reader.ReadInt32();
            texStream.Seek(texDataOffset, SeekOrigin.Begin);

            Unk04 = reader.ReadByte();
            MipMapCount = reader.ReadByte();
            Format = (ArmorTextureFormat)reader.ReadByte();
            Unk07 = reader.ReadByte();
            Unk08 = reader.ReadByte();
            Unk09 = reader.ReadByte();
            Unk0A = reader.ReadByte();
            Unk0B = reader.ReadByte();
            Unk0C = reader.ReadByte();
            Unk0D = reader.ReadByte();
            Unk0E = reader.ReadByte();
            Unk0F = reader.ReadByte();
            GTFFlags = reader.ReadInt32();
            Unk14 = reader.ReadByte();
            Unk15 = reader.ReadByte();
            Unk16 = reader.ReadByte();
            Unk17 = reader.ReadByte();
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            Unk1C = reader.ReadInt16();
            Unk1E = reader.ReadInt16();
            Unk20 = reader.ReadInt16();
            Unk22 = reader.ReadInt16();

            switch (Format)
            {
                case ArmorTextureFormat.BC1:
                    for (int level = 0; level < MipMapCount; level++)
                    {
                        short mipWidth = (short)Math.Max(1, Width >> level); 
                        short mipHeight = (short)Math.Max(1, Height >> level);

                        int blocksX = (int)Math.Ceiling((double)mipWidth / 4);
                        int blocksY = (int)Math.Ceiling((double)mipHeight / 4);

                        long mipmapSize = blocksX * blocksY * 8;

                        MipMap mip = new MipMap();
                        mip.Width = mipWidth;
                        mip.Height = mipHeight;
                        mip.MipData = new byte[mipmapSize];
                        texStream.Read(mip.MipData, 0, (int)mipmapSize);
                        MipMaps.Add(mip);
                    }
                    break;
                case ArmorTextureFormat.BC2:
                case ArmorTextureFormat.BC3:
                    for (int level = 0; level < MipMapCount; level++)
                    {
                        short mipWidth = (short)Math.Max(1, Width >> level);
                        short mipHeight = (short)Math.Max(1, Height >> level);

                        int blocksX = (int)Math.Ceiling((double)mipWidth / 4);
                        int blocksY = (int)Math.Ceiling((double)mipHeight / 4);

                        long mipmapSize = blocksX * blocksY * 16;

                        MipMap mip = new MipMap();
                        mip.Width = mipWidth;
                        mip.Height = mipHeight;
                        mip.MipData = new byte[mipmapSize];
                        texStream.Read(mip.MipData, 0, (int)mipmapSize);
                        MipMaps.Add(mip);
                    }
                    break;
            }
        }

        public void Write(EndianBinaryWriter writer, Stream texStream)
        {
            writer.Write(texStream.Position);
            writer.Write(Unk04);
            writer.Write(MipMapCount);
            writer.Write((byte)Format);
            writer.Write(Unk07);
            writer.Write(Unk08);
            writer.Write(Unk09);
            writer.Write(Unk0A);
            writer.Write(Unk0B);
            writer.Write(Unk0C);
            writer.Write(Unk0D);
            writer.Write(Unk0E);
            writer.Write(Unk0F);
            writer.Write(GTFFlags);
            writer.Write(Unk14);
            writer.Write(Unk15);
            writer.Write(Unk16);
            writer.Write(Unk17);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(Unk1C);
            writer.Write(Unk1E);
            writer.Write(Unk20);
            writer.Write(Unk22);
            foreach (var mip in MipMaps)
            {
                texStream.Write(mip.MipData);
            }

            if (texStream.Position % 16 != 0)
            {
                byte[] tBuf = new byte[texStream.Position - (texStream.Position % 16)];
                texStream.Write(tBuf);
            }
        }

        public ArmorTexture()
        {
            MipMaps = new List<MipMap>();
        }
    }

    public struct ArmorVertex 
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;
        public Vector4 Weights;
        public Vector4 Indices;
    }


    public struct Bone
    {
        public Matrix4x4 BindPoseMatrix;
        public Vector3 Position;
        public short Flags;
        public short Parent;
    }


    public class Armor
    {
        public List<ArmorTexturedSubmesh> TexturedMeshes;
        public List<ArmorReflectiveSubmesh> ReflectiveMeshes;  // these are transparent overlay meshes which get an env shine applied.
        public List<ArmorTexture> Textures;
        public ArmorVertex[]? Vertices;
        public List<Bone> Bones;
        private Stream? mTexStream;

        public bool IsPS3;

        private void Read(EndianBinaryReader reader)
        {
            int meshInfoOffset  = reader.ReadInt32();
            // we can use this to test
            if (meshInfoOffset > 0x10)
            {
                reader.Endian = Endianness.Big;
                IsPS3 = true;
            }
            // correct endian;
            meshInfoOffset = BinaryPrimitives.ReverseEndianness(meshInfoOffset);
            int textureInfoOffset = reader.ReadInt32();
            int textureCount = reader.ReadInt32();
            int pad = reader.ReadInt32();

            int texturedMeshCount = reader.ReadInt32();
            int reflectiveMeshCount = reader.ReadInt32();
            int texturedMeshInfoOffset = reader.ReadInt32();
            int reflectiveMeshInfoOffset = reader.ReadInt32();

            int verticesOffset = reader.ReadInt32();
            int indicesOffset = reader.ReadInt32();

            short texturedVerticesCount = reader.ReadInt16();
            short reflectiveVerticesCount = reader.ReadInt16();
            Vertices = new ArmorVertex[texturedVerticesCount + reflectiveVerticesCount];

            short indicesCount = reader.ReadInt16();
            // 2 bytes padding it to the end

            reader.Seek(texturedMeshInfoOffset, SeekOrigin.Begin);

            for (int i = 0; i < texturedMeshCount; i++)
            {
                ArmorTexturedSubmesh mesh = new ArmorTexturedSubmesh();
                mesh.TextureIndex = reader.ReadInt32();
                int baseFaceIndex = reader.ReadInt32();
                mesh.IndexCount = reader.ReadInt32();
                mesh.Indices = new ushort[mesh.IndexCount];
                mesh.Flags = reader.ReadInt32();
                Console.WriteLine($"Mesh:\n\tTexture Index: {mesh.TextureIndex}\n\tIndex Count: {mesh.IndexCount}\n\tFlags: {mesh.Flags}");
                TexturedMeshes.Add(mesh);
            }

            reader.Seek(reflectiveMeshInfoOffset, SeekOrigin.Begin);

            for (int i = 0; i < reflectiveMeshCount; i++)
            {
                ArmorReflectiveSubmesh mesh = new ArmorReflectiveSubmesh();
                mesh.ReflectionMode = reader.ReadInt32();
                int baseFaceIndex = reader.ReadInt32();
                mesh.IndexCount = reader.ReadInt32();
                mesh.Indices = new ushort[mesh.IndexCount];
                mesh.Flags = reader.ReadInt32();
                Console.WriteLine($"Reflection Mesh:\n\tTexture Index: {mesh.ReflectionMode}\n\tIndex Count: {mesh.IndexCount}\n\tFlags: {mesh.Flags}");
                ReflectiveMeshes.Add(mesh);
            }

            // next we'll fetch the vertices

            reader.Seek(verticesOffset, SeekOrigin.Begin);

            for (int i = 0; i < texturedVerticesCount + reflectiveVerticesCount; i++)
            {
                Vertices[i].Position = reader.ReadVec3();
                if (IsPS3)
                {
                    Vertices[i].Normal = reader.ReadVec3();
                }
                else
                {
                    Vertices[i].Normal = reader.ReadVec3(VectorBinaryFormat.SInt8Normalized);
                    reader.Seek(1, SeekOrigin.Current);
                }
                if (i < texturedVerticesCount)
                    Vertices[i].UV = reader.ReadVec2();
                Vertices[i].Weights = reader.ReadVec4(VectorBinaryFormat.UInt8Normalized);
                Vertices[i].Indices = reader.ReadVec4(VectorBinaryFormat.UInt8);
            }

            // next fetch the indices

            reader.Seek(indicesOffset, SeekOrigin.Begin);

            // sanity check
            int indexReadCount = 0;

            foreach (var mesh in TexturedMeshes)
            {
                if (mesh.Indices != null)
                {
                    Console.WriteLine("T");
                    for (int i = 0; i < mesh.IndexCount; i++)
                    {
                        mesh.Indices[i] = reader.ReadUInt16();
                        indexReadCount++;
                    }
                }
            }

            foreach (var mesh in ReflectiveMeshes)
            {
                if (mesh.Indices != null)
                {
                    for (int i = 0; i < mesh.IndexCount; i++)
                    {
                        mesh.Indices[i] = reader.ReadUInt16();
                        indexReadCount++;
                    }
                }
            }

            reader.Seek(textureInfoOffset, SeekOrigin.Begin);

            if (mTexStream != null)
            {
                for (int i = 0; i < textureCount; i++)
                {
                    ArmorTexture tex = new ArmorTexture();
                    tex.Read(reader, mTexStream);
                    Textures.Add(tex);
                }
            }
        }

        public void Load(string filePath, string? enginePath = null)
        {
            Console.WriteLine(enginePath == null ? "null" : enginePath);
            string? dirPath = Path.GetDirectoryName(filePath);
            string fName = Path.GetFileNameWithoutExtension(filePath);
            string? texPath = null;

            if (dirPath != null)
            {
                texPath = Path.Join(dirPath, fName + ".vram");
            }

            if (texPath != null)
            {
                mTexStream = File.Open(texPath, FileMode.Open);
                Read(new EndianBinaryReader(File.Open(filePath, FileMode.Open)));
            }
            else
            {
                throw new FileNotFoundException($"Unable to find matching VRAM file for {filePath}");
            }

            if (enginePath != null)
            {
                using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(enginePath, FileMode.Open)))
                {
                    if (IsPS3)
                        reader.Endian = Endianness.Big;
                    int mobyOffset = reader.ReadInt32();
                    reader.Seek(mobyOffset, SeekOrigin.Begin);
                    int mobyCount = reader.ReadInt32();
                    int ratchetOffset = 0;
                    for (int i = 0; i < mobyCount; i++)
                    {
                        int mobyID = reader.ReadInt32();
                        ratchetOffset = reader.ReadInt32();
                        if (mobyID == 0)
                        {
                            break;
                        }
                    }

                    reader.Seek(ratchetOffset, SeekOrigin.Begin);

                    int modelOffset = reader.ReadInt32();  // will always be 0 in RaC 2/3.
                    int nulls1 = reader.ReadInt32();
                    byte boneCount = reader.ReadByte();
                    byte lpBoneCount = reader.ReadByte();  // number of low poly bones.
                    reader.Seek(10, SeekOrigin.Current);  // skip to the bone data
                    long pos = reader.BaseStream.Position;
                    Console.WriteLine($"0x{pos:X8}");
                    int boneMatrixOffset = reader.ReadInt32();
                    int boneInfoOffset = reader.ReadInt32();
                    for (int j = 0; j < boneCount; j++)
                    {
                        Bone bone = new Bone();
                        reader.Seek(ratchetOffset + boneMatrixOffset, SeekOrigin.Begin);

                        bone.BindPoseMatrix = new Matrix4x4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                                                            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                                                            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                                                            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                        bone.BindPoseMatrix.M41 = 0;  // the 4th row for some reason contains the inverse of the position, but this breaks things for most render engines, so simplify.
                        bone.BindPoseMatrix.M42 = 0;
                        bone.BindPoseMatrix.M43 = 0;
                        bone.BindPoseMatrix.M44 = 1;

                        boneMatrixOffset += 0x40;
                        reader.Seek(ratchetOffset + boneInfoOffset, SeekOrigin.Begin);
                        bone.Position = reader.ReadVec3();
                        bone.Flags = reader.ReadInt16();
                        bone.Parent = (short)(reader.ReadInt16() / (short)(0x40));
                        boneInfoOffset += 0x10;

                        Bones.Add(bone);
                    }
                }
            }
        }

        public Armor()
        {
            TexturedMeshes = new List<ArmorTexturedSubmesh>();
            ReflectiveMeshes = new List<ArmorReflectiveSubmesh>();
            Textures = new List<ArmorTexture>();
            Bones = new List<Bone>();
            mTexStream = null;
        }
    }
}
