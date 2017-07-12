using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Java.Lang;

namespace Emmaus.Widget
{
    public class RecyclerViewPagerAdapter : RecyclerView.Adapter
    {

        private readonly RecyclerViewPager mViewPager;
        public RecyclerView.Adapter mAdapter;
        public override int ItemCount {
            get
            {
                return mAdapter.ItemCount;
            }
        }


        
        public RecyclerViewPagerAdapter(RecyclerViewPager viewPager,RecyclerView.Adapter adapter)
        {
            mAdapter = adapter;
            mViewPager = viewPager;
            HasStableIds = mAdapter.HasStableIds;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            mAdapter.OnBindViewHolder(holder, position);
            View itemView = holder.ItemView;
            ViewGroup.LayoutParams lp;
            if (itemView.LayoutParameters == null)
            {
                lp = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent
                    , ViewGroup.LayoutParams.MatchParent);
            }
            else
            {
                lp = itemView.LayoutParameters;
                if (mViewPager.GetLayoutManager().CanScrollHorizontally())
                {
                    lp.Width = ViewGroup.LayoutParams.MatchParent;
                }
                else
                {
                    lp.Height = ViewGroup.LayoutParams.MatchParent;
                }
            }
            itemView.LayoutParameters = lp;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            return mAdapter.OnCreateViewHolder(parent, viewType);
        }

        public override void RegisterAdapterDataObserver(RecyclerView.AdapterDataObserver observer)
        {
            //base.RegisterAdapterDataObserver(observer);
            mAdapter.RegisterAdapterDataObserver(observer);
        }

        public override void UnregisterAdapterDataObserver(RecyclerView.AdapterDataObserver observer)
        {
            base.UnregisterAdapterDataObserver(observer);
            mAdapter.UnregisterAdapterDataObserver(observer);
        }
        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            base.OnViewRecycled(holder);
            mAdapter.OnViewRecycled(holder);
        }

        public override bool OnFailedToRecycleView(Java.Lang.Object holder)
        {
            return mAdapter.OnFailedToRecycleView(holder);
        }

        public override void OnViewAttachedToWindow(Java.Lang.Object holder)
        {
            base.OnViewAttachedToWindow(holder);
            mAdapter.OnViewAttachedToWindow(holder);
        }

        public override void OnViewDetachedFromWindow(Java.Lang.Object holder)
        {
            base.OnViewDetachedFromWindow(holder);
            mAdapter.OnViewDetachedFromWindow(holder);
        }

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            base.OnAttachedToRecyclerView(recyclerView);
            mAdapter.OnAttachedToRecyclerView(recyclerView);
        }

        public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
        {
            base.OnDetachedFromRecyclerView(recyclerView);
            mAdapter.OnDetachedFromRecyclerView(recyclerView);
        }

        public override long GetItemId(int position)
        {
            return mAdapter.GetItemId(position);
        }

        public override int GetItemViewType(int position)
        {
            return mAdapter.GetItemViewType(position);
        }
    }

}