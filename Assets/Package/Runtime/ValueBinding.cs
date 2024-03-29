﻿using System;

namespace Flui
{
    public class ValueBinding<TDataType> : IValueBinding
    {
        private readonly Func<TDataType> _modelValueGetter;
        private readonly Action<TDataType> _modelValueSetter;
        private readonly Func<TDataType> _viewValueGetter;
        private readonly Action<TDataType> _viewValueSetter;
        private readonly Action _onModelChanged;
        private TDataType _previousModelValue;
        private TDataType _previousViewValue;
        private Func<bool> _lockedFunc;

        public ValueBinding(
            Func<TDataType> modelValueGetter,
            Action<TDataType> modelValueSetter,
            Func<TDataType> viewValueGetter,
            Action<TDataType> viewValueSetter,
            Action onModelChanged = null)
        {
            _modelValueGetter = modelValueGetter;
            _modelValueSetter = modelValueSetter;
            _viewValueGetter = viewValueGetter;
            _viewValueSetter = viewValueSetter;
            _onModelChanged = onModelChanged;

            SetViewValue(_modelValueGetter());
        }

        public bool HasError { get; private set; }
        public ValueBindingChange ChangeOnLastCheck { get; private set; }

        public void Update()
        {
            if (_lockedFunc?.Invoke() == true)
            {
                return;
            }

            HasError = false;
            var currentModelValue = _modelValueGetter();
            ChangeOnLastCheck = ValueBindingChange.None;
            if (!Equals(currentModelValue, _previousModelValue))
            {
                SetViewValue(currentModelValue);
                ChangeOnLastCheck = ValueBindingChange.ModelValueChanged;
                return;
            }

            if (_modelValueSetter == null)
            {
                return;
            }

            var currentViewValue = _viewValueGetter();
            if (!Equals(currentViewValue, _previousViewValue))
            {
                SetModelValue(currentViewValue);
                ChangeOnLastCheck = ValueBindingChange.ViewValueChanged;
            }
        }

        private void SetViewValue(TDataType modelValue)
        {
            HasError = false;
            try
            {
                ValueBindingStats.BindingSetViewValueCount++;
                _previousModelValue = modelValue;
                _previousViewValue = modelValue;
                _viewValueSetter(modelValue);
            }
            catch
            {
                HasError = true;
            }
        }

        private void SetModelValue(TDataType viewValue)
        {
            HasError = false;
            try
            {
                ValueBindingStats.BindingSetModelValueCount++;
                _previousViewValue = viewValue;
                _previousModelValue = viewValue;
                _modelValueSetter(_previousModelValue);
                _onModelChanged?.Invoke();
            }
            catch
            {
                HasError = true;
            }
        }

        public ValueBinding<TDataType> SetLockedFunc(Func<bool> lockedFunc)
        {
            _lockedFunc = lockedFunc;
            return this;
        }
    }
}