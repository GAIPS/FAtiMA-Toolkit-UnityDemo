using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Animation
{
	public class Face2DController : FaceController
	{
		[Serializable]
		private class FacialExpression
		{
			public string Name;
			public Sprite[] _intencitySprites;
		}

		[SerializeField]
		private SpriteRenderer _renderer;

		[SerializeField]
		private Sprite _defaultExpression;

		[SerializeField]
		private FacialExpression[] _facialExpressions;

		private void Awake()
		{
			_renderer.sprite = _defaultExpression;
		}

		protected override IFacialExpressionImplementation CreateFacialExpressionImplementation(string name)
		{
			var face = _facialExpressions.FirstOrDefault(f => f.Name == name);
			if(face == null)
				throw new Exception(string.Format("No facial expression found for \"{0}\"", name));

			return new FacialController(_renderer, face,_defaultExpression);
		}

		protected override string GetFacialExpressionNameAtIndex(int index)
		{
			return _facialExpressions[index].Name;
		}

		private sealed class FacialController : IFacialExpressionImplementation
		{
			private SpriteRenderer _mesh;
			private FacialExpression _facial;
			private Sprite _defaultSprite;

			public FacialController(SpriteRenderer mesh, FacialExpression exp, Sprite neutralSprite)
			{
				_mesh = mesh;
				_facial = exp;
				_defaultSprite = neutralSprite;
			}

			public void SetAmount(float amount)
			{
				int i;
				try
				{
					i = Mathf.RoundToInt(_facial._intencitySprites.Length * amount - 1);
					if (i < 0)
						_mesh.sprite = _defaultSprite;
					else
						_mesh.sprite = _facial._intencitySprites[i];
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}
	}
}