using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
    /// <summary>
    /// UIParticle 性能分析器
    /// 统计每帧 BakeMesh、CombineMesh、SetMesh 的耗时，以及 Canvas Rebatch 消耗等关键指标
    /// </summary>
    public static class UIParticleProfiler
    {
        // 计时相关
        private static double _bakeMeshTimeMs;
        private static double _combineMeshTimeMs;
        private static double _setMeshTimeMs;
        private static double _totalTimeMs;

        // 统计数据
        private static int _activeUIParticleCount;
        private static int _totalParticleCount;
        private static int _uniqueMaterialCount;
        private static int _canvasGraphicCount;
        private static double _canvasRebatchTimeMs;

        // 材质收集
        private static readonly List<Material> s_Materials = new List<Material>(64);
        private static readonly HashSet<int> s_MaterialIds = new HashSet<int>();

        /// <summary>
        /// 活跃的 UIParticle 数量
        /// </summary>
        public static int activeUIParticleCount => _activeUIParticleCount;

        /// <summary>
        /// 总粒子数量
        /// </summary>
        public static int totalParticleCount => _totalParticleCount;

        /// <summary>
        /// 使用的唯一材质数量
        /// </summary>
        public static int uniqueMaterialCount => _uniqueMaterialCount;

        /// <summary>
        /// Canvas 中的 Graphic 组件数量
        /// </summary>
        public static int canvasGraphicCount => _canvasGraphicCount;

        /// <summary>
        /// Canvas Rebatch 耗时 (ms)
        /// </summary>
        public static double canvasRebatchTimeMs => _canvasRebatchTimeMs;

        /// <summary>
        /// BakeMesh 耗时 (ms)
        /// </summary>
        public static double bakeMeshTimeMs => _bakeMeshTimeMs;

        /// <summary>
        /// CombineMesh 耗时 (ms)
        /// </summary>
        public static double combineMeshTimeMs => _combineMeshTimeMs;

        /// <summary>
        /// SetMesh 耗时 (ms)
        /// </summary>
        public static double setMeshTimeMs => _setMeshTimeMs;

        /// <summary>
        /// CPU 总耗时 (ms)
        /// </summary>
        public static double totalTimeMs => _totalTimeMs;

        /// <summary>
        /// 开始帧统计
        /// </summary>
        public static void BeginFrame()
        {
            Profiler.BeginSample("[UIParticleProfiler] BeginFrame");

            // 重置帧数据
            _bakeMeshTimeMs = 0;
            _combineMeshTimeMs = 0;
            _setMeshTimeMs = 0;
            _totalTimeMs = 0;
            _activeUIParticleCount = 0;
            _totalParticleCount = 0;
            _uniqueMaterialCount = 0;
            _canvasGraphicCount = 0;
            _canvasRebatchTimeMs = 0;

            s_Materials.Clear();
            s_MaterialIds.Clear();

            Profiler.EndSample();
        }

        /// <summary>
        /// 结束帧统计
        /// </summary>
        public static void EndFrame()
        {
            Profiler.BeginSample("[UIParticleProfiler] EndFrame");

            // 计算总耗时
            _totalTimeMs = _bakeMeshTimeMs + _combineMeshTimeMs + _setMeshTimeMs;

            // 统计唯一材质数量
            foreach (var mat in s_Materials)
            {
                if (mat != null)
                {
                    s_MaterialIds.Add(mat.GetInstanceID());
                }
            }
            _uniqueMaterialCount = s_MaterialIds.Count;

            Profiler.EndSample();
        }

        /// <summary>
        /// 添加 BakeMesh 耗时
        /// </summary>
        public static void AddBakeMeshTime(double timeMs)
        {
            _bakeMeshTimeMs += timeMs;
        }

        /// <summary>
        /// 添加 CombineMesh 耗时
        /// </summary>
        public static void AddCombineMeshTime(double timeMs)
        {
            _combineMeshTimeMs += timeMs;
        }

        /// <summary>
        /// 添加 SetMesh 耗时
        /// </summary>
        public static void AddSetMeshTime(double timeMs)
        {
            _setMeshTimeMs += timeMs;
        }

        /// <summary>
        /// 添加 Canvas Rebatch 耗时
        /// </summary>
        public static void AddCanvasRebatchTime(double timeMs)
        {
            _canvasRebatchTimeMs += timeMs;
        }

        /// <summary>
        /// 注册 UIParticle 数据
        /// </summary>
        public static void RegisterUIParticle(UIParticle particle, ParticleSystem ps)
        {
            if (particle == null || !particle.isActiveAndEnabled)
                return;

            _activeUIParticleCount++;

            if (ps != null)
            {
                _totalParticleCount += ps.particleCount;
            }
        }

        /// <summary>
        /// 注册材质
        /// </summary>
        public static void RegisterMaterial(Material material)
        {
            if (material != null)
            {
                s_Materials.Add(material);
            }
        }

        /// <summary>
        /// 统计 Canvas 中的 Graphic 数量
        /// </summary>
        public static void CountCanvasGraphics(Canvas canvas)
        {
            if (canvas == null)
                return;

            var graphics = InternalListPool<Graphic>.Rent();
            canvas.GetComponentsInChildren<Graphic>(true, graphics);
            _canvasGraphicCount = graphics.Count;
            InternalListPool<Graphic>.Return(ref graphics);
        }

        /// <summary>
        /// 测量 Canvas Rebatch 耗时
        /// </summary>
        public static void MeasureCanvasRebatch(Canvas canvas, System.Action action)
        {
            if (action == null)
            {
                return;
            }

            var startTime = Time.realtimeSinceStartupAsDouble;
            action();
            var endTime = Time.realtimeSinceStartupAsDouble;

            var elapsedMs = (endTime - startTime) * 1000.0;
            _canvasRebatchTimeMs += elapsedMs;
        }

        /// <summary>
        /// 重置所有统计数据
        /// </summary>
        public static void Reset()
        {
            _bakeMeshTimeMs = 0;
            _combineMeshTimeMs = 0;
            _setMeshTimeMs = 0;
            _totalTimeMs = 0;
            _activeUIParticleCount = 0;
            _totalParticleCount = 0;
            _uniqueMaterialCount = 0;
            _canvasGraphicCount = 0;
            _canvasRebatchTimeMs = 0;

            s_Materials.Clear();
            s_MaterialIds.Clear();
        }
    }
}
