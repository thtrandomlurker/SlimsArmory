using Assimp;
using Assimp.Configs;
using Assimp.Unmanaged;
using OpenTK.Mathematics;
using RaCLib.IO;
using RaCLib.Armor;
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

                    aiMat.Name = $"{armorName}-mat{i}_tex_{ps3Armor.TexturedMeshes[i].TextureIndex}";

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

                    aiMat.Name = $"{armorName}-mat{i}_ref_{ps3Armor.ReflectiveMeshes[i].ReflectionMode}";

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
                return;
            }

            if (argList.Contains("-c"))
            {


                Armor ps3Armor = new Armor();

                string? outputDir = Path.GetDirectoryName(Path.GetFullPath(argList.Last()));
                string? outputFileName;

                if (argList.Contains("-o"))
                {
                    outputFileName = argList[argList.IndexOf("-o") + 1];
                    outputDir = Path.GetDirectoryName(Path.GetFullPath(outputFileName));
                }
                else
                {
                    outputFileName = Path.Join(Path.GetDirectoryName(Path.GetFullPath(argList.Last())), Path.GetFileNameWithoutExtension(argList.Last()) + ".ps3");
                }
                
                Console.WriteLine(outputFileName);

                AssimpContext context = new AssimpContext();
                context.SetConfig(new VertexBoneWeightLimitConfig(4));
                Scene scene = context.ImportFile(argList.Last(), PostProcessSteps.OptimizeMeshes | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate);

                List<Mesh> texturedMeshes = new List<Mesh>();
                List<Mesh> reflectionMeshes = new List<Mesh>();

                List<int> matIndexTexMap = new List<int>();
                List<int> matRefModeMap = new List<int>();

                foreach (var mesh in scene.Meshes)
                {
                    Console.WriteLine($"{mesh.Name}");

                    string nameToParse = scene.Materials[mesh.MaterialIndex].Name;
                    string[] nameParts = nameToParse.Substring(nameToParse.IndexOf('-')).Split('_');
                    Console.Write(nameParts[2]);

                    if (nameParts[1] == "ref")
                    {
                        ps3Armor.NumReflectiveVertices += (short)mesh.VertexCount;
                        matRefModeMap.Add(int.Parse(nameToParse.Substring(nameToParse.IndexOf('-')).Split('_').Last()));
                        reflectionMeshes.Add(mesh);
                    }
                    else
                    {
                        ps3Armor.NumTexturedVertices += (short)mesh.VertexCount;
                        ps3Armor.NumLitVertices += (short)mesh.VertexCount;
                        texturedMeshes.Add(mesh);
                        if (!matIndexTexMap.Contains(mesh.MaterialIndex))
                            matIndexTexMap.Add(mesh.MaterialIndex);
                    }
                }
                ps3Armor.Vertices = new ArmorVertex[ps3Armor.NumTexturedVertices + ps3Armor.NumReflectiveVertices];


                int baseVertIndex = 0;
                int baseIndex = 0;

                foreach (var texMeshTmp in texturedMeshes)
                {
                    ArmorTexturedSubmesh texMesh = new ArmorTexturedSubmesh();
                    texMesh.TextureIndex = matIndexTexMap.IndexOf(texMeshTmp.MaterialIndex);
                    texMesh.StartIndex = baseIndex;
                    texMesh.IndexCount = texMeshTmp.FaceCount * 3;
                    texMesh.Flags = 0x05000000;
                    texMesh.Indices = new ushort[texMesh.IndexCount];

                    int indexPos = 0;

                    foreach (var face in texMeshTmp.Faces)
                    {
                        foreach (var idx in face.Indices)
                        {
                            texMesh.Indices[indexPos] = (ushort)((ushort)idx + (ushort)baseVertIndex);
                            indexPos += 1;
                        }
                    }
                    ps3Armor.TexturedMeshes.Add(texMesh);

                    baseIndex += texMesh.IndexCount;
                    // now put it into the vxbf

                    int[][] tIndices = new int[texMeshTmp.VertexCount][];
                    float[][] tWeights = new float[texMeshTmp.VertexCount][];
                    int[] tIndicesCurPos = new int[texMeshTmp.VertexCount];

                    for (int i = 0; i < texMeshTmp.VertexCount; i++)
                    {
                        tIndices[i] = new int[4];
                        tWeights[i] = new float[4];
                    }

                    foreach (var bone in texMeshTmp.Bones)
                    {
                        Console.WriteLine(bone.Name);
                        foreach (var weight in bone.VertexWeights)
                        {
                            Console.WriteLine(weight.VertexID);
                            tIndices[weight.VertexID][tIndicesCurPos[weight.VertexID]] = int.Parse(bone.Name.Split('_').Last());
                            tWeights[weight.VertexID][tIndicesCurPos[weight.VertexID]] = weight.Weight;
                            tIndicesCurPos[weight.VertexID]++;
                        }
                    }


                    for (int i = 0; i < texMeshTmp.VertexCount; i++)
                    {
                        ArmorVertex vertex = new ArmorVertex();
                        vertex.Position = texMeshTmp.Vertices[i] / 1024.0f;
                        vertex.Normal = texMeshTmp.Normals[i];
                        vertex.UV.X = texMeshTmp.TextureCoordinateChannels[0][i].X;
                        vertex.UV.Y = texMeshTmp.TextureCoordinateChannels[0][i].Y * -1.0f;
                        vertex.Weights.X = tWeights[i][0];
                        vertex.Weights.Y = tWeights[i][1];
                        vertex.Weights.Z = tWeights[i][2];
                        vertex.Weights.W = tWeights[i][3];
                        vertex.Indices.X = tIndices[i][0];
                        vertex.Indices.Y = tIndices[i][1];
                        vertex.Indices.Z = tIndices[i][2];
                        vertex.Indices.W = tIndices[i][3];

                        ps3Armor.Vertices[i + baseVertIndex] = vertex;
                    }
                    baseVertIndex += texMeshTmp.VertexCount;
                }

                foreach (var refMeshTmp in reflectionMeshes)
                {
                    ArmorReflectiveSubmesh refMesh = new ArmorReflectiveSubmesh();
                    refMesh.ReflectionMode = matRefModeMap[ps3Armor.ReflectiveMeshes.Count];
                    refMesh.StartIndex = baseIndex;
                    refMesh.IndexCount = refMeshTmp.FaceCount * 3;
                    refMesh.Flags = 0x05000000;
                    refMesh.Indices = new ushort[refMesh.IndexCount];

                    int indexPos = 0;

                    foreach (var face in refMeshTmp.Faces)
                    {
                        foreach (var idx in face.Indices)
                        {
                            refMesh.Indices[indexPos] = (ushort)((ushort)idx + (ushort)baseVertIndex);
                            indexPos += 1;
                        }
                    }
                    ps3Armor.ReflectiveMeshes.Add(refMesh);

                    baseIndex += refMesh.IndexCount;
                    // now put it into the vxbf

                    int[][] tIndices = new int[refMeshTmp.VertexCount][];
                    float[][] tWeights = new float[refMeshTmp.VertexCount][];
                    int[] tIndicesCurPos = new int[refMeshTmp.VertexCount];

                    for (int i = 0; i < refMeshTmp.VertexCount; i++)
                    {
                        tIndices[i] = new int[4];
                        tWeights[i] = new float[4];
                    }

                    foreach (var bone in refMeshTmp.Bones)
                    {
                        Console.WriteLine(bone.Name);
                        foreach (var weight in bone.VertexWeights)
                        {
                            Console.WriteLine(weight.VertexID);
                            tIndices[weight.VertexID][tIndicesCurPos[weight.VertexID]] = int.Parse(bone.Name.Split('_').Last());
                            tWeights[weight.VertexID][tIndicesCurPos[weight.VertexID]] = weight.Weight;
                            tIndicesCurPos[weight.VertexID]++;
                        }
                    }


                    for (int i = 0; i < refMeshTmp.VertexCount; i++)
                    {
                        ArmorVertex vertex = new ArmorVertex();
                        vertex.Position = refMeshTmp.Vertices[i] / 1024.0f;
                        vertex.Normal = refMeshTmp.Normals[i];
                        vertex.Weights.X = tWeights[i][0];
                        vertex.Weights.Y = tWeights[i][1];
                        vertex.Weights.Z = tWeights[i][2];
                        vertex.Weights.W = tWeights[i][3];
                        vertex.Indices.X = tIndices[i][0];
                        vertex.Indices.Y = tIndices[i][1];
                        vertex.Indices.Z = tIndices[i][2];
                        vertex.Indices.W = tIndices[i][3];

                        ps3Armor.Vertices[i + baseVertIndex] = vertex;
                    }
                    baseVertIndex += refMeshTmp.VertexCount;
                }

                // now we can initialize the vertex array

                string platform = argList[argList.IndexOf("-c") + 1];
                if (platform == "ps3")
                {
                    ps3Armor.IsPS3 = true;
                }
                else if (platform != "vita")
                {
                    throw new ArgumentException($"Unsupported platform {platform}");
                }

                for (int i = 0; i < matIndexTexMap.Count; i++)
                {
                    ArmorTexture tex = new ArmorTexture();

                    string sourcePath = scene.Materials[matIndexTexMap[i]].TextureDiffuse.FilePath;

                    if (sourcePath == "")
                    {
                        throw new InvalidDataException("Textured mat has no texture");
                    }

                    if (!Path.IsPathFullyQualified(sourcePath))
                    {
                        sourcePath = Path.Join(Path.GetDirectoryName(Path.GetFullPath(argList.Last())), sourcePath);
                    }

                    Console.WriteLine(sourcePath);

                    if (Path.GetExtension(sourcePath) == ".dds")
                    {
                        using (EndianBinaryReader ddsReader = new EndianBinaryReader(File.OpenRead(sourcePath)))
                        {
                            ddsReader.Seek(0xC, SeekOrigin.Begin);
                            tex.Width = (short)ddsReader.ReadInt32();
                            tex.Height = (short)ddsReader.ReadInt32();
                            int pitch = ddsReader.ReadInt32();
                            ddsReader.Seek(0x4, SeekOrigin.Current);
                            tex.MipMapCount = (byte)ddsReader.ReadInt32();
                            ddsReader.Seek(0x54, SeekOrigin.Begin);
                            int fourcc = (int)ddsReader.ReadInt32();

                            switch (fourcc)
                            {
                                case 0x31545844:
                                    tex.Format = ArmorTextureFormat.BC1;
                                    ddsReader.Seek(0x80, SeekOrigin.Begin);
                                    for (int level = 0; level < tex.MipMapCount; level++)
                                    {
                                        short mipWidth = (short)Math.Max(1, tex.Width >> level);
                                        short mipHeight = (short)Math.Max(1, tex.Height >> level);

                                        int blocksX = (int)Math.Ceiling((double)mipWidth / 4);
                                        int blocksY = (int)Math.Ceiling((double)mipHeight / 4);

                                        long mipmapSize = blocksX * blocksY * 8;

                                        MipMap mip = new MipMap();
                                        mip.Width = mipWidth;
                                        mip.Height = mipHeight;
                                        mip.MipData = new byte[mipmapSize];
                                        ddsReader.Read(mip.MipData, 0, (int)mipmapSize);
                                        tex.MipMaps.Add(mip);
                                    }
                                    break;
                                case 0x33545844:
                                    tex.Format = ArmorTextureFormat.BC2;
                                    ddsReader.Seek(0x80, SeekOrigin.Begin);
                                    for (int level = 0; level < tex.MipMapCount; level++)
                                    {
                                        short mipWidth = (short)Math.Max(1, tex.Width >> level);
                                        short mipHeight = (short)Math.Max(1, tex.Height >> level);

                                        int blocksX = (int)Math.Ceiling((double)mipWidth / 4);
                                        int blocksY = (int)Math.Ceiling((double)mipHeight / 4);

                                        long mipmapSize = blocksX * blocksY * 16;

                                        MipMap mip = new MipMap();
                                        mip.Width = mipWidth;
                                        mip.Height = mipHeight;
                                        mip.MipData = new byte[mipmapSize];
                                        ddsReader.Read(mip.MipData, 0, (int)mipmapSize);
                                        tex.MipMaps.Add(mip);
                                    }
                                    break;
                                case 0x35545844:
                                    tex.Format = ArmorTextureFormat.BC3;
                                    ddsReader.Seek(0x80, SeekOrigin.Begin);
                                    for (int level = 0; level < tex.MipMapCount; level++)
                                    {
                                        short mipWidth = (short)Math.Max(1, tex.Width >> level);
                                        short mipHeight = (short)Math.Max(1, tex.Height >> level);

                                        int blocksX = (int)Math.Ceiling((double)mipWidth / 4);
                                        int blocksY = (int)Math.Ceiling((double)mipHeight / 4);

                                        long mipmapSize = blocksX * blocksY * 16;

                                        MipMap mip = new MipMap();
                                        mip.Width = mipWidth;
                                        mip.Height = mipHeight;
                                        mip.MipData = new byte[mipmapSize];
                                        ddsReader.Read(mip.MipData, 0, (int)mipmapSize);
                                        tex.MipMaps.Add(mip);
                                    }
                                    break;
                                default:
                                    throw new InvalidDataException("Unsupported Texture Format");
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidDataException("Non-DDS Textures are currently unsupported");
                    }
                    ps3Armor.Textures.Add(tex);
                }

                ps3Armor.Save(outputFileName);
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
            return;
        }

        else
        {
            Console.WriteLine(
@"Usage: SlimsArmory [OPTIONS]... file
    -e              Extracts a model to a DAE (Collada) File
    -c              Creates a model from a DAE File
    -v              View a specified model
    -o              Specifies output filename (Defaults to Input filename if not specified)");
            return;
        }
    }
}