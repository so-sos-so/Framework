﻿using System;
using System.Collections.Specialized;
using Framework.Assets;
using Framework.Asynchronous;
using Framework.UI.Core;
using Framework.UI.Core.Bind;
using Framework.UI.Wrap.Base;
using Tool;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.UI.Wrap
{
    public class ViewWrapper: BaseWrapper<View>, IBindList<ViewModel>
    {
        private readonly Transform _content;
        private readonly View _item;
        private readonly GameObject _template;
        private int _tag;
        private int _index;
        private IRes _res;

        public ViewWrapper(View view,Transform root, int index = 0) : base(view)
        {
            _res = new AddressableRes();
            _item =  view;
            _content = root;
            Log.Assert(_content.childCount == 1 , "_content.childCount 只能有一个");
            _template = _content.GetChild(0).gameObject;
            _template.ActiveHide();
            _tag = 0;
            _index = index;
        }

        public void SetTag(int tag)
        {
            this._tag = tag;
        }

        Action<NotifyCollectionChangedAction, ViewModel, int> IBindList<ViewModel>.GetBindListFunc()
        {
            return BindListFunc;
        }

        private void BindListFunc
            (NotifyCollectionChangedAction type, ViewModel newViewModel, int index)
        {
            var tag = GetTag(newViewModel);
            if (tag != this._tag) return;
            switch (type)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItem(index, newViewModel);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItem(index);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ReplaceItem(index, newViewModel);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    break;
                case NotifyCollectionChangedAction.Move: break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void AddItem(int index, ViewModel vm)
        {
            var view = ReflectionHelper.CreateInstance(_item.GetCLRType()) as View;
            var go = Object.Instantiate(_template);
            go.transform.SetParent(_content);
            go.transform.SetAsLastSibling();
            go.ActiveShow();
            view.SetGameObject(go);
            view.SetVm(vm);
            view.Show();
            _index = index;
        }

        private void RemoveItem(int index)
        {
            Object.Destroy(_content.GetChild(index).gameObject);
        }

        private void ReplaceItem(int index, ViewModel vm)
        {
            RemoveItem(index);
            AddItem(index, vm);
        }

        private void Clear()
        {
            while (_content.childCount > 1)
            {
                RemoveItem(1);
            }
        }

        private static int GetTag(ViewModel vm)
        {
            var _vm = vm as IBindMulView;
            if (_vm == null) return 0;
            return _vm.Tag;
        }
    }
}