using Assimp;
using Assimp.Unmanaged;
using OpenTK.Mathematics;
using RaCLib.Armor;
using RaCLib.IO;
using SlimsArmory;
using SlimsArmory.AssimpHelpers;
using System.ComponentModel.Design;
using System.Data;
using System.Numerics;

using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

static class Program
{
    static void Main(string[] args)
    {
        List<string> argList = args.ToList();
        if (argList.Count >= 1)
        {
            if (argList.Contains("-e"))
            {
                if (argList.Contains("-c"))
                {
                    Console.WriteLine("ERROR: Cannot create when exporting model");
                    return;
                }

                string? outputDir = Path.GetDirectoryName(Path.GetFullPath(argList.Last()));
                string? outputFileName;

                if (argList.Contains("-o"))
                {
                    outputFileName = argList[argList.IndexOf("-o") + 1];
                    outputDir = Path.GetDirectoryName(Path.GetFullPath(outputFileName));
                }
                else
                {
                    outputFileName = Path.Join(Path.GetDirectoryName(Path.GetFullPath(argList.Last())), Path.GetFileNameWithoutExtension(argList.Last()) + ".dae");
                }
                Armor ps3Armor = new Armor();

                string? filePath = Path.GetDirectoryName(Path.GetFullPath(argList.Last()));

                string engPath = Path.Join(filePath, "..\\..\\level0\\engine.ps3");

                string armorName = Path.GetFileNameWithoutExtension(argList.Last());


                Console.WriteLine(engPath);
                Console.WriteLine(Path.Exists(engPath));

                ps3Armor.Load(argList.Last(), engPath);

                // dump the file for safety

                int baseIndex = 1;

                AssimpContext context = new AssimpContext();

                Scene scene = new Scene();

                scene.RootNode = new Node("Armor");

                if (ps3Armor.Bones.Count != 0)
                {
                    for (int i = 0; i < ps3Armor.Bones.Count; i++)
                    {
                        Node boneToNode = new Node($"Bone_{i:D3}");

                        boneToNode.Transform = ps3Armor.Bones[i].BindPoseMatrix;
                        if (ps3Armor.Bones[i].Flags == 0x7000)
                        {
                            scene.RootNode.FindNode($"Bone_{(ps3Armor.Bones[i].Parent):D3}").Children.Add(boneToNode);
                        }
                        else
                        {
                            scene.RootNode.Children.Add(boneToNode);
                        }
                    }
                }
                foreach (var fmt in context.GetSupportedExportFormats())
                {
                    Console.WriteLine(fmt.FileExtension);
                }

                for (int i = 0; i < ps3Armor.TexturedMeshes.Count; i++)
                {
                    Mesh aiMesh = new Mesh($"TexMesh_{i}");

                    Material aiMat = new Material();

                    aiMat.Name = $"{armorName}_mat_{i}_tex_{ps3Armor.TexturedMeshes[i].TextureIndex}";

                    TextureSlot diffTex = new TextureSlot() { FilePath = $"{armorName}_tex_{ps3Armor.TexturedMeshes[i].TextureIndex}.dds" };
                    diffTex.Mapping = TextureMapping.FromUV;
                    diffTex.UVIndex = 0;
                    diffTex.TextureType = TextureType.Diffuse;

                    aiMat.AddMaterialTexture(diffTex);

                    aiMat.ShadingMode = ShadingMode.Blinn;

                    scene.Materials.Add(aiMat);
                    aiMesh.MaterialIndex = i;

                    for (int j = 0; j < ps3Armor.Bones.Count; j++)
                    {
                        Assimp.Bone aiBone = new Assimp.Bone();
                        aiBone.Name = $"Bone_{j:D3}";
                        Matrix4x4 offsetMatrix = AssimpHelpers.CalculateNodeMatrixWS(scene.RootNode.FindNode($"Bone_{j:D3}"));
                        Matrix4x4.Invert(offsetMatrix, out Matrix4x4 inverseBindPoseMatrix);

                        aiBone.OffsetMatrix = inverseBindPoseMatrix;

                        aiMesh.Bones.Add(aiBone);
                    }


                    for (int j = 0; j < ps3Armor.TexturedMeshes[i].IndexCount; j += 3)
                    {
                        aiMesh.Vertices.Add(ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j]].Position * 1024.0f);
                        aiMesh.Vertices.Add(ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 1]].Position * 1024.0f);
                        aiMesh.Vertices.Add(ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 2]].Position * 1024.0f);
                        aiMesh.Normals.Add(ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j]].Normal);
                        aiMesh.Normals.Add(ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 1]].Normal);
                        aiMesh.Normals.Add(ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 2]].Normal);
                        Vector3 UV0 = new Vector3(0.0f, 0.0f, 0.0f);
                        Vector3 UV1 = new Vector3(0.0f, 0.0f, 0.0f);
                        Vector3 UV2 = new Vector3(0.0f, 0.0f, 0.0f);
                        UV0.X = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j]].UV.X;
                        UV0.Y = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j]].UV.Y * -1.0f;
                        UV1.X = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 1]].UV.X;
                        UV1.Y = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 1]].UV.Y * -1.0f;
                        UV2.X = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 2]].UV.X;
                        UV2.Y = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 2]].UV.Y * -1.0f;

                        aiMesh.TextureCoordinateChannels[0].Add(UV0);
                        aiMesh.TextureCoordinateChannels[0].Add(UV1);
                        aiMesh.TextureCoordinateChannels[0].Add(UV2);

                        // determine if the face is counter or not.

                        // start by generating the face normal
                        Vector3 edge0 = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 1]].Position - ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j]].Position;
                        Vector3 edge1 = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 2]].Position - ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j]].Position;

                        Vector3 faceNormal = Vector3.Cross(edge0, edge1);

                        // then average the vertex normal to get an estimate of the correct normal direction
                        Vector3 vertexNormalAvg = (ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j]].Normal + ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 1]].Normal + ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 2]].Normal) / 3.0f;

                        // do a dot product to compare the two
                        float winding = Vector3.Dot(faceNormal, vertexNormalAvg);


                        // and if it's positive, it's correct
                        if (winding > 0)
                        {

                            Face aiFace = new Face(new int[] { j, j + 1, j + 2 });
                            aiMesh.Faces.Add(aiFace);
                        }
                        // else, it's been reversed, and we'll now un-reverse it.
                        else
                        {
                            Face aiFace = new Face(new int[] { j + 2, j + 1, j });
                            aiMesh.Faces.Add(aiFace);
                        }

                        if (ps3Armor.Bones.Count != 0)
                        {
                            // weights
                            Vector4 tIndices0 = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j]].Indices;
                            Vector4 tWeights0 = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j]].Weights;
                            Vector4 tIndices1 = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 1]].Indices;
                            Vector4 tWeights1 = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 1]].Weights;
                            Vector4 tIndices2 = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 2]].Indices;
                            Vector4 tWeights2 = ps3Armor.Vertices[ps3Armor.TexturedMeshes[i].Indices[j + 2]].Weights;
                            for (int k = 0; k < 4; k++)
                            {
                                aiMesh.Bones[(int)tIndices0[k]].VertexWeights.Add(new VertexWeight() { VertexID = j, Weight = tWeights0[k] });
                                aiMesh.Bones[(int)tIndices1[k]].VertexWeights.Add(new VertexWeight() { VertexID = j + 1, Weight = tWeights1[k] });
                                aiMesh.Bones[(int)tIndices2[k]].VertexWeights.Add(new VertexWeight() { VertexID = j + 2, Weight = tWeights2[k] });
                            }
                        }
                    }

                    scene.Meshes.Add(aiMesh);
                    Node meshNode = new Node($"Mesh_{i}");
                    meshNode.MeshIndices.Add(i);
                    scene.RootNode.Children.Add(meshNode);
                }

                // Reflection Meshes

                for (int i = 0; i < ps3Armor.ReflectiveMeshes.Count; i++)
                {
                    Mesh aiMesh = new Mesh($"RefMesh_{i}");

                    Material aiMat = new Material();

                    aiMat.Name = $"{armorName}_mat_{i}_ref_{ps3Armor.ReflectiveMeshes[i].ReflectionMode}";

                    aiMat.ShadingMode = ShadingMode.Blinn;

                    aiMat.ColorDiffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    aiMat.ColorSpecular = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

                    scene.Materials.Add(aiMat);
                    aiMesh.MaterialIndex = i + ps3Armor.TexturedMeshes.Count;

                    for (int j = 0; j < ps3Armor.Bones.Count; j++)
                    {
                        Assimp.Bone aiBone = new Assimp.Bone();
                        aiBone.Name = $"Bone_{j:D3}";
                        Matrix4x4 offsetMatrix = AssimpHelpers.CalculateNodeMatrixWS(scene.RootNode.FindNode($"Bone_{j:D3}"));
                        Matrix4x4.Invert(offsetMatrix, out Matrix4x4 inverseBindPoseMatrix);

                        aiBone.OffsetMatrix = inverseBindPoseMatrix;

                        aiMesh.Bones.Add(aiBone);
                    }


                    for (int j = 0; j < ps3Armor.ReflectiveMeshes[i].IndexCount; j += 3)
                    {
                        aiMesh.Vertices.Add(ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j]].Position * 1024.0f);
                        aiMesh.Vertices.Add(ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 1]].Position * 1024.0f);
                        aiMesh.Vertices.Add(ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 2]].Position * 1024.0f);
                        aiMesh.Normals.Add(ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j]].Normal);
                        aiMesh.Normals.Add(ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 1]].Normal);
                        aiMesh.Normals.Add(ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 2]].Normal);

                        // determine if the face is counter or not.

                        // start by generating the face normal
                        Vector3 edge0 = ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 1]].Position - ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j]].Position;
                        Vector3 edge1 = ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 2]].Position - ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j]].Position;

                        Vector3 faceNormal = Vector3.Cross(edge0, edge1);

                        // then average the vertex normal to get an estimate of the correct normal direction
                        Vector3 vertexNormalAvg = (ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j]].Normal + ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 1]].Normal + ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 2]].Normal) / 3.0f;

                        // do a dot product to compare the two
                        float winding = Vector3.Dot(faceNormal, vertexNormalAvg);


                        // and if it's positive, it's correct
                        if (winding > 0)
                        {

                            Face aiFace = new Face(new int[] { j, j + 1, j + 2 });
                            aiMesh.Faces.Add(aiFace);
                        }
                        // else, it's been reversed, and we'll now un-reverse it.
                        else
                        {
                            Face aiFace = new Face(new int[] { j + 2, j + 1, j });
                            aiMesh.Faces.Add(aiFace);
                        }

                        if (ps3Armor.Bones.Count != 0)
                        {
                            // weights
                            Vector4 tIndices0 = ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j]].Indices;
                            Vector4 tWeights0 = ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j]].Weights;
                            Vector4 tIndices1 = ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 1]].Indices;
                            Vector4 tWeights1 = ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 1]].Weights;
                            Vector4 tIndices2 = ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 2]].Indices;
                            Vector4 tWeights2 = ps3Armor.Vertices[ps3Armor.ReflectiveMeshes[i].Indices[j + 2]].Weights;
                            for (int k = 0; k < 4; k++)
                            {
                                aiMesh.Bones[(int)tIndices0[k]].VertexWeights.Add(new VertexWeight() { VertexID = j, Weight = tWeights0[k] });
                                aiMesh.Bones[(int)tIndices1[k]].VertexWeights.Add(new VertexWeight() { VertexID = j + 1, Weight = tWeights1[k] });
                                aiMesh.Bones[(int)tIndices2[k]].VertexWeights.Add(new VertexWeight() { VertexID = j + 2, Weight = tWeights2[k] });
                            }
                        }
                    }

                    scene.Meshes.Add(aiMesh);
                    Node meshNode = new Node($"RefMesh_{i}");
                    meshNode.MeshIndices.Add(i + ps3Armor.TexturedMeshes.Count);
                    scene.RootNode.Children.Add(meshNode);
                }
                context.ExportFile(scene, outputFileName, "collada", PostProcessSteps.JoinIdenticalVertices);

                // dump textures

                for (int i = 0; i < ps3Armor.Textures.Count; i++)
                {
                    using (EndianBinaryWriter writer = new EndianBinaryWriter(File.Create(Path.Join(outputDir, $"{armorName}_tex_{i}.dds"))))
                    {
                        writer.Write(0x20534444);
                        writer.Write(0x7C);
                        writer.Write(0x000A1007);
                        writer.Write((int)ps3Armor.Textures[i].Width);
                        writer.Write((int)ps3Armor.Textures[i].Height);
                        writer.Write(ps3Armor.Textures[i].Format == ArmorTextureFormat.BC1 ? ps3Armor.Textures[i].Width * ps3Armor.Textures[i].Height / 2 : ps3Armor.Textures[i].Width * ps3Armor.Textures[i].Height);
                        writer.Write(1);
                        writer.Write((int)ps3Armor.Textures[i].MipMapCount);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);

                        writer.Write(0x20);
                        writer.Write(4);

                        switch (ps3Armor.Textures[i].Format)
                        {
                            case ArmorTextureFormat.BC1:
                                writer.Write(0x31545844);
                                break;
                            case ArmorTextureFormat.BC2:
                                writer.Write(0x33545844);
                                break;
                            case ArmorTextureFormat.BC3:
                                writer.Write(0x35545844);
                                break;
                            default:
                                throw new InvalidDataException("Unknown format");
                        }

                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);

                        writer.Write(0x00401008);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);

                        foreach (var mip in ps3Armor.Textures[i].MipMaps)
                        {
                            writer.Write(mip.MipData);
                        }

                    }
                }
            }

            if (argList.Contains("-c"))
            {
                Console.WriteLine("WIP");
                return;
            }

            if (argList.Contains("-v"))
            {
                if (argList.Contains("-e"))
                {
                    Console.WriteLine("Cannot extract model when viewing");
                    return;
                }

                if (argList.Contains("-c"))
                {
                    Console.WriteLine("Cannot create model when viewing");
                    return;
                }


                Armor ps3Armor = new Armor();

                string? filePath = Path.GetDirectoryName(Path.GetFullPath(argList.Last()));

                string engPath = Path.Join(filePath, "..\\..\\level0\\engine.ps3");

                string armorName = Path.GetFileNameWithoutExtension(argList.Last());


                Console.WriteLine(engPath);
                Console.WriteLine(Path.Exists(engPath));

                ps3Armor.Load(argList.Last(), engPath);


                using (RenderWindow wnd = new RenderWindow(1280, 720, ps3Armor))
                {
                    wnd.Run();
                }
            }
        }

        else
        {
            Console.WriteLine(
@"Usage: SlimsArmory [OPTIONS]... file
    -e              Extracts a model to a DAE (Collada) File
    -c              Creates a model from a DAE File
    -v              View a specified model
    -o              Specifies output filename (Defaults to Input filename if not specified)");
        }
    }
}