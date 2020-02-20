﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Framework.UI.Wrap;
using Framework.UI.Core;
using UnityEngine;

namespace Framework.UI.Core
{
    public class BindList<TVm> where TVm : ViewModel
    {
        private Transform content;
        private List<View> views;
        private BindableList<TVm> list;
        private List<ViewWrapper> wrappers;

        public BindList (BindableList<TVm> _list, params View[] _view)
        {
            SetValue(_list, _view);
            InitEvent();
            InitCpntValue();
        }

        private void InitCpntValue()
        {
            for (int i = 0; i < list.Count; i++)
            {
                var vm = list[i];
                wrappers.ForEach((wrapper) =>
                    ((IBindList<ViewModel>) wrapper).GetBindListFunc()(NotifyCollectionChangedAction.Add, null, vm, i));

            }
        }

        public void SetValue(BindableList<TVm> _list, params View[] _view)
        {
            views = _view.ToList ();
            content = views[0].transform.parent;
            list = _list;
        }

        private void InitEvent()
        {
            wrappers = new List<ViewWrapper> (views.Count);
            for ( int i = 0; i < views.Count; i++ )
            {
                var wrapper = new ViewWrapper(views[i]);
                wrapper.SetTag(i);
                list.AddListener(((IBindList<ViewModel>) wrapper).GetBindListFunc());
                views[i].Hide();
                wrappers.Add(wrapper);
            }
        }
    }

    public class BindIpairsView<TVm> where TVm : ViewModel
    {
        private BindableList<TVm> list;
        private List<View> views;

        public BindIpairsView (ref BindableList<TVm> _list, string itemName, Transform root)
        {
            SetValue(ref _list, itemName, root);
        }

        public void SetValue(ref BindableList<TVm> _list, string itemName, Transform root)
        {
            list = _list;
            ParseItems (itemName, root);
            InitEvent();
        }

        private void ParseItems (string itemName, Transform root)
        {
            views = new List<View> ();
            Regex regex = new Regex (@"[/w ]*?(?<=\[)[?](?=\])");
            if ( !regex.IsMatch (itemName) )
            {
                Debug.LogError ($"{itemName} not match (skill[?]) pattern.");
                return;
            }
            Transform upTransform = null;
            for ( int i = 0; i < Int32.MaxValue; i++ )
            {
                string item = regex.Replace (itemName, i.ToString ());
                View view;
                if ( upTransform == null )
                {
                    
                    view = root.FindInAllChild (item)?.GetComponent<View> ();
                    upTransform = view.transform.parent;
                }
                else
                {
                    view = upTransform.Find (item)?.GetComponent<View> ();
                    if ( view == null )
                    {
                        view = root.FindInAllChild (item)?.GetComponent<View> ();
                    }
                }
                if(view == null) break;
                views.Add(view);
            }
        }

        private void InitEvent()
        {
            for (int i = 0; i < views.Count; i++)
            {
                views[i].SetVM(list[i]);
            }
        }
    }
}