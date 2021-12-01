using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapGenerator;

public class influenceMap : MonoBehaviour
{

    public struct Vector2I
    {
        public int x;
        public int y;
        public float d;

        public Vector2I(int nx, int ny)
        {
            x = nx;
            y = ny;
            d = 1;
        }

        public Vector2I(int nx, int ny, float nd)
        {
            x = nx;
            y = ny;
            d = nd;
        }
    }

    public class InfluenceMap
    {

        List<UnitType> _propagators = new List<UnitType>();

        float[,] _influences;
        float[,] _influencesBuffer;
        public float Decay { get; set; }
        public float Momentum { get; set; }
        public int Width { get { return _influences.GetLength(0); } }
        public int Height { get { return _influences.GetLength(1); } }

        public float GetValue(int x, int y)
        {
            return _influences[x, y];
        }

        public InfluenceMap(int width, int height, float decay, float momentum)
        {
            _influences = new float[width, height];
            _influencesBuffer = new float[width, height];
            Decay = decay;
            Momentum = momentum;
        }

        public void SetInfluence(int x, int y, float value)
        {
            if (x < Width && y < Height)
            {
                _influences[x, y] = value;
                _influencesBuffer[x, y] = value;
            }
        }

        public void SetInfluence(Vector2I pos, float value)
        {
            if (pos.x < Width && pos.y < Height)
            {
                _influences[pos.x, pos.y] = value;
                _influencesBuffer[pos.x, pos.y] = value;
            }
        }

        public void RegisterPropagator(UnitType p)
        {
            _propagators.Add(p);
        }

        public void Propagate()
        {
            UpdatePropagators();
            UpdatePropagation();
            UpdateInfluenceBuffer();
        }

        void UpdatePropagators()
        {
            foreach (UnitType p in _propagators)
            {
                SetInfluence(new Vector2I ((int)p.position.x, (int)p.position.y), p.influenceValue);
            }
        }
        void UpdatePropagation()
        {
            for (int xIdx = 0; xIdx < _influences.GetLength(0); ++xIdx)
            {
                for (int yIdx = 0; yIdx < _influences.GetLength(1); ++yIdx)
                {
                    //Debug.Log("at " + xIdx + ", " + yIdx);
                    float maxInf = 0.0f;
                    float minInf = 0.0f;
                    Vector2I[] neighbors = GetNeighbors(xIdx, yIdx);
                    foreach (Vector2I n in neighbors)
                    {
                        //Debug.Log(n.x + " " + n.y);
                        float inf = _influencesBuffer[n.x, n.y] * Mathf.Exp(-Decay * n.d) * Decay;
                        maxInf = Mathf.Max(inf, maxInf);
                        minInf = Mathf.Min(inf, minInf);
                    }

                    if (Mathf.Abs(minInf) > maxInf)
                    {
                        _influences[xIdx, yIdx] = Mathf.Lerp(_influencesBuffer[xIdx, yIdx], minInf, Momentum);
                    }
                    else
                    {
                        _influences[xIdx, yIdx] = Mathf.Lerp(_influencesBuffer[xIdx, yIdx], maxInf, Momentum);
                    }
                }
            }
        }

        void UpdateInfluenceBuffer()
        {
            for (int xIdx = 0; xIdx < _influences.GetLength(0); ++xIdx)
            {
                for (int yIdx = 0; yIdx < _influences.GetLength(1); ++yIdx)
                {
                    _influencesBuffer[xIdx, yIdx] = _influences[xIdx, yIdx];
                }
            }
        }

        Vector2I[] GetNeighbors(int x, int y)
        {
            List<Vector2I> retVal = new List<Vector2I>();

            // comprobar el limite izquierdo
            if (x > 0)
            {
                retVal.Add(new Vector2I(x - 1, y));
            }

            // comprobar el limite derecho
            if (x < _influences.GetLength(0) - 1)
            {
                retVal.Add(new Vector2I(x + 1, y));
            }

            // comprobar el limite abajo
            if (y > 0)
            {
                retVal.Add(new Vector2I(x, y - 1));
            }

            // comprobar el limite arriba
            if (y < _influences.GetLength(1) - 1)
            {
                retVal.Add(new Vector2I(x, y + 1));
            }


            // diagonales

            // comprobar el limite abajo izquierda
            if (x > 0 && y > 0)
            {
                retVal.Add(new Vector2I(x - 1, y - 1, 1.4142f));
            }

            // comprobar el limite arriba derecha
            if (x < _influences.GetLength(0) - 1 && y < _influences.GetLength(1) - 1)
            {
                retVal.Add(new Vector2I(x + 1, y + 1, 1.4142f));
            }

            // comprobar el limite arriba izquierda
            if (x > 0 && y < _influences.GetLength(1) - 1)
            {
                retVal.Add(new Vector2I(x - 1, y + 1, 1.4142f));
            }

            // comprobar el limite abajo derecha
            if (x < _influences.GetLength(0) - 1 && y > 0)
            {
                retVal.Add(new Vector2I(x + 1, y - 1, 1.4142f));
            }

            return retVal.ToArray();
        }

    }
}
