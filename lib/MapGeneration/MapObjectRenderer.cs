using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace nv
{
    public class MapObjectRenderer
    {
        public Bounds boundingVolume;

        Mesh instanceMesh;
        Material instanceMaterial;
        int subMeshIndex;

        //int cachedInstanceCount = -1;
        //int cachedSubMeshIndex = -1;
        ComputeBuffer posBuffer;
        ComputeBuffer rotBuffer;
        ComputeBuffer argsBuffer;
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        YieldInstruction waitForEndOfFrame;
        MaterialPropertyBlock matProperties;
        int renderLayer = -1;
        //Camera renderTarget;
        
        public MapObjectRenderer(Mesh instanceMesh, Material instanceMaterial, int subMeshIndex = 0)
        {
            this.instanceMesh = instanceMesh;
            this.instanceMaterial = instanceMaterial;
            this.subMeshIndex = subMeshIndex;
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            // Ensure submesh index is in range
            if(instanceMesh != null)
                subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

            waitForEndOfFrame = new WaitForEndOfFrame();
            matProperties = new MaterialPropertyBlock();

            //TODO: allow these to be set by the user
            renderLayer = LayerMask.NameToLayer("Default");
            //renderTarget = Camera.main;
        }

        /// <summary>
        /// Needs to be called in a Monobehavior's Update() to render correctly
        /// </summary>
        public void RenderObjects()
        {
            Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, boundingVolume, argsBuffer, 0, matProperties, UnityEngine.Rendering.ShadowCastingMode.Off, false, renderLayer);
        }

        public void SetRenderData(List<Vector3> positions, List<float> scales = null, List<Vector4> rotations = null)
        {
            if(rotations != null)
            {
                //error, need identical sized sets of data
                if(positions.Count != rotations.Count)
                    return;
            }
            else
            {
                rotations = (new Vector4[positions.Count]).ToList();
                rotations = rotations.Select(x => new Vector4(Quaternion.identity.x, Quaternion.identity.y, Quaternion.identity.z, Quaternion.identity.w)).ToList();
            }

            if(scales != null)
            {
                if(positions.Count != scales.Count)
                    return;
            }
            else
            {
                scales = (new float[positions.Count]).ToList();
                scales = scales.Select(x => 100f).ToList();
            }

            if(posBuffer != null)
                posBuffer.Release();

            int instanceCount = positions.Count;

            posBuffer = new ComputeBuffer(instanceCount, 16);
            rotBuffer = new ComputeBuffer(instanceCount, 16);

            Vector4[] renderPosData = new Vector4[instanceCount];
            for(int i = 0; i < instanceCount; i++)
            {
                renderPosData[i] = new Vector4(positions[i].x, positions[i].y, positions[i].z, scales[i]);
            }

            posBuffer.SetData(renderPosData);
            rotBuffer.SetData(rotations);

            instanceMaterial.SetBuffer("positionBuffer", posBuffer);
            instanceMaterial.SetBuffer("rotationBuffer", rotBuffer);
            
            // Indirect args
            if(instanceMesh != null)
            {
                args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
                args[1] = (uint)instanceCount;
                args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
                args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
            }
            else
            {
                args[0] = args[1] = args[2] = args[3] = 0;
            }
            argsBuffer.SetData(args);

            //need to set the compute buffers in the material property block for the draw call to work properly
            matProperties.SetBuffer("positionBuffer", posBuffer);
            matProperties.SetBuffer("rotationBuffer", rotBuffer);
        }

        public void FreeMemory()
        {
            if(posBuffer != null)
                posBuffer.Release();
            posBuffer = null;

            if(rotBuffer != null)
                rotBuffer.Release();
            rotBuffer = null;

            if(argsBuffer != null)
                argsBuffer.Release();
            argsBuffer = null;
        }
    }
}
