using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//namespace nv
//{
//    [Serializable]
//    public struct MeshElement
//    {
//        public static MeshElement EmptyElement
//        {
//            get
//            {
//                return new MeshElement();
//            }
//        }

//        public void CopyIDFrom(MeshElement other)
//        {
//            id = other.id;
//        }

//        public bool ContainsID(List<Color> mask)
//        {
//            if(mask.Contains(id))
//                return true;
//            return false;
//        }

//        public bool Empty
//        {
//            get
//            {
//                return (ID0 | ID1 | ID2 | ID3) == 0;
//            }
//        }

//        public Vector3 wpos
//        {
//            get
//            {
//                Vector3 v = new Vector3(PosX, 0, PosY);
//                return v;
//            }
//        }

//        public Vector3 wposition
//        {
//            get
//            {
//                Vector3 v = new Vector3(PosX, 0, PosY);
//                return v;
//            }
//        }

//        public Vector3 wEdgePosition
//        {
//            get
//            {
//                return wposition + new Vector3(.5f, 0, .5f);
//            }
//        }

//        public Vector3 wxEdgePosition
//        {
//            get
//            {
//                return wposition + new Vector3(.5f, 0, 0);
//            }
//        }

//        public Vector3 wyEdgePosition
//        {
//            get
//            {
//                return wposition + new Vector3(0, 0, .5f);
//            }
//        }

//        public Vector2 position
//        {
//            get
//            {
//                Vector2 v = new Vector2(PosX, PosY);
//                return v;
//            }
//            set
//            {
//                PosX = value.x;
//                PosY = value.y;
//            }
//        }

//        public Vector2 EdgePosition
//        {
//            get
//            {
//                return position + Vector2.one * 0.5f;
//            }
//        }

//        public Vector2 xEdgePosition
//        {
//            get
//            {
//                return position + new Vector2(.5f, 0);
//            }
//        }

//        public Vector2 yEdgePosition
//        {
//            get
//            {
//                return position + new Vector2(0, .5f);
//            }
//        }

//        public float x
//        {
//            get
//            {
//                return PosX;
//            }
//            set
//            {
//                PosX = value;
//            }
//        }

//        public float y
//        {
//            get
//            {
//                return PosY;
//            }
//            set
//            {
//                PosY = value;
//            }
//        }

//        public Color32 id
//        {
//            get
//            {
//                Color32 c = new Color32(ID0, ID1, ID2, ID3);
//                return c;
//            }
//            set
//            {
//                ID0 = value.r;
//                ID1 = value.g;
//                ID2 = value.b;
//                ID3 = value.a;
//            }
//        }

//        //Use to easily set individual id elements
//        public byte this[int c]
//        {
//            get
//            {
//                c = Mathf.Clamp(c, 0, 3);
//                switch(c)
//                {
//                    case 0: return ID0;
//                    case 1: return ID1;
//                    case 2: return ID2;
//                    case 3: return ID3;
//                    default: return 0;
//                }
//            }
//            set
//            {
//                c = Mathf.Clamp(c, 0, 3);
//                switch(c)
//                {
//                    case 0: ID0 = value; break;
//                    case 1: ID1 = value; break;
//                    case 2: ID2 = value; break;
//                    case 3: ID3 = value; break;
//                    default: break;
//                }
//            }
//        }

//        public Vector2 pos
//        {
//            get
//            {
//                Vector2 v = new Vector2(PosX, PosY);
//                return v;
//            }
//            set
//            {
//                PosX = value.x;
//                PosY = value.y;
//            }
//        }

//        public static bool CompareID(MeshElement a, MeshElement b)
//        {
//            return a[0] == b[0] && a[1] == b[1] && a[2] == b[2] && a[3] == b[3];
//        }

//        [SerializeField]
//        float PosX;
//        [SerializeField]
//        float PosY;

//        [SerializeField]
//        byte ID0;
//        [SerializeField]
//        byte ID1;
//        [SerializeField]
//        byte ID2;
//        [SerializeField]
//        byte ID3;
//    }
//}