using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Animation
{
	public class Face3DController : FaceController
	{
		[SerializeField]
		private SkinnedMeshRenderer _faceMeshRenderer = null;

		[Space]
		[SerializeField]
		private FacialExpression[] _facialExpressions;

		[Serializable]
		private class FacialExpression
		{
			[SerializeField]
			private string _expressionName;
			[SerializeField]
			private SkinBlend[] _blends;

			public string ExpressionName {
				get { return _expressionName; }
			}

			public void SetBlends(SkinnedMeshRenderer mesh, float amount)
			{
				foreach (var b in _blends)
					mesh.SetBlendShapeWeight(b.BlendShapeId, b.Weight * amount);
			}
		}

		[Serializable]
		private class SkinBlend
		{
			public int BlendShapeId = 0;

			[Range(0,100)]
			public float Weight = 50f;
		}

		public IExpressionController GetExpressionController(int index)
		{
			return GetExpressionController(_facialExpressions[index].ExpressionName);
		}

		protected override IFacialExpressionImplementation CreateFacialExpressionImplementation(string name)
		{
			var facial = _facialExpressions.FirstOrDefault(e => e.ExpressionName == name);
			if (facial == null)
				throw new Exception(string.Format("No facial expression found for \"{0}\"", name));

			return new FacialController(_faceMeshRenderer,facial);
		}

		protected override string GetFacialExpressionNameAtIndex(int index)
		{
			return _facialExpressions[index].ExpressionName;
		}

		private sealed class FacialController : IFacialExpressionImplementation
		{
			private SkinnedMeshRenderer _mesh;
			private FacialExpression _facial;

			public FacialController(SkinnedMeshRenderer mesh, FacialExpression exp)
			{
				_mesh = mesh;
				_facial = exp;
			}

			public void SetAmount(float amount)
			{
				_facial.SetBlends(_mesh,amount);
			}
		}
	}
}
