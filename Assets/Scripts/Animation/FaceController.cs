using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Animation
{
	public abstract class FaceController : MonoBehaviour
	{
		[SerializeField]
		[Range(0.0001f, 1f)]
		private float _blendSmoothDamping = 0.1f;

		private Dictionary<string, IExpressionController> _instatiatedControllers = new Dictionary<string, IExpressionController>();

		public IExpressionController GetExpressionController(string name)
		{
			IExpressionController c;
			if (_instatiatedControllers.TryGetValue(name, out c))
				return c;

			var facial = CreateFacialExpressionImplementation(name);
			if (facial == null)
				throw new NullReferenceException();

			c = new FacialExpressionController(this, facial);
			_instatiatedControllers.Add(name, c);
			return c;
		}

		public IExpressionController GetExpressionController(int index)
		{
			return GetExpressionController(GetFacialExpressionNameAtIndex(index));
		}

		protected abstract IFacialExpressionImplementation CreateFacialExpressionImplementation(string name);

		protected abstract string GetFacialExpressionNameAtIndex(int index);

		public interface IExpressionController
		{
			float Amount { get; set; }
			float TargetAmount { get; set; }
		}

		protected interface IFacialExpressionImplementation
		{
			void SetAmount(float amount);
		}

		private class FacialExpressionController : IExpressionController
		{
			private IFacialExpressionImplementation _expression;
			private FaceController _parent;

			private float _amount;
			private float _targetAmount;
			private Coroutine _transitionHandle = null;

			public FacialExpressionController(FaceController parent, IFacialExpressionImplementation exp)
			{
				_parent = parent;
				_expression = exp;

				_targetAmount = _amount = 0;
				_expression.SetAmount(0);
			}

			public float Amount
			{
				get { return _amount; }
				set
				{
					if (_transitionHandle != null)
					{
						_parent.StopCoroutine(_transitionHandle);
						_transitionHandle = null;
					}

					_amount = value;
					_expression.SetAmount(_amount);
				}
			}

			public float TargetAmount
			{
				get { return _targetAmount; }
				set
				{
					_targetAmount = value;
					if (_transitionHandle == null)
						_transitionHandle = _parent.StartCoroutine(AnimationCoroutine());
				}
			}

			private IEnumerator AnimationCoroutine()
			{
				float speed = 0;
				while (Mathf.Abs(_targetAmount - _amount) > 0.001f)
				{
					yield return null;
					_amount = Mathf.SmoothDamp(_amount, _targetAmount, ref speed, _parent._blendSmoothDamping, float.MaxValue, Time.smoothDeltaTime);
					_expression.SetAmount(_amount);
				}

				_amount = _targetAmount;
				_expression.SetAmount(_amount);
				_transitionHandle = null;
			}
		}
	}
}