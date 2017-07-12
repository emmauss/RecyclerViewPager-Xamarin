using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Util;
using Android.Widget;
using Android.Support.V7;
using Android.Support.V7.Widget;

namespace Emmaus.Widget
{
    public class LoopRecyclerViewPager : RecyclerViewPager
    {
        public LoopRecyclerViewPager(Context context) : this(context,null)
        {
            
        }

        public LoopRecyclerViewPager(Context context, IAttributeSet attrs) : this(context,attrs,0)
        {
            
        }

        public LoopRecyclerViewPager(Context context, IAttributeSet attrs, int defStyle) 
            : base(context,attrs,defStyle)
        {
           
        }

        public override void SetAdapter(Adapter adapter)
        {
            base.SetAdapter(adapter);
            base.ScrollToPosition(GetMiddlePosition());
        }

        public override void SwapAdapter(Adapter adapter, bool removeAndRecycleExistingViews)
        {
            base.SwapAdapter(adapter, removeAndRecycleExistingViews);
            base.ScrollToPosition(GetMiddlePosition());
        }

        public override RecyclerViewPagerAdapter EnsureRecyclerViewPagerAdapter(Adapter adapter)
        {
            return (adapter is LoopRecyclerViewPagerAdapter)
                ? (LoopRecyclerViewPagerAdapter)adapter
                : new LoopRecyclerViewPagerAdapter(this, adapter);
        }

        /// <summary>
        /// Starts a smooth scroll to an adapter position.if position is less  than adapter.getActualCount
        ///position will be transform to right position.
        /// </summary>
        /// <param name="position">target position</param>
        public override void SmoothScrollToPosition(int position)
        {
            base.SmoothScrollToPosition(position);
        }

        /// <summary>
        /// get actual current position in actual adapter.
        /// </summary>
        /// <returns></returns>
        public int GetActualCurrentPosition()
        {
            int position = CurrentPosition;
            return TransformToActualPosition(position);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns> </returns>
        public int TransformToActualPosition(int position)
        {
            if (GetAdapter() == null || GetAdapter().ItemCount < 0)
            {
                return 0;
            }
            return position % GetActualItemCountFromAdapter();
        }

        private int GetActualItemCountFromAdapter()
        {
            return ((LoopRecyclerViewPagerAdapter)GetWrapperAdapter()).GetActualItemCount();
        }

        private int transformInnerPositionIfNeed(int position)
        {
            int actualItemCount = GetActualItemCountFromAdapter();
            if (actualItemCount == 0)
                return actualItemCount;
            int actualCurrentPosition = CurrentPosition % actualItemCount;
            int bakPosition1 = CurrentPosition
                    - actualCurrentPosition
                    + position % actualItemCount;
            int bakPosition2 = CurrentPosition
                    - actualCurrentPosition
                    - actualItemCount
                    + position % actualItemCount;
            int bakPosition3 = CurrentPosition
                    - actualCurrentPosition
                    + actualItemCount
                    + position % actualItemCount;
            Log.Error("test", bakPosition1 + "/" + bakPosition2 + "/" + bakPosition3 + "/" + CurrentPosition);
            // get position which is closer to current position
            if (Math.Abs(bakPosition1 - CurrentPosition) > Math.Abs(bakPosition2 -
                    CurrentPosition))
            {
                if (Math.Abs(bakPosition2 -
                        CurrentPosition) > Math.Abs(bakPosition3 -
                        CurrentPosition))
                {
                    return bakPosition3;
                }
                return bakPosition2;
            }
            else
            {
                if (Math.Abs(bakPosition1 -
                        CurrentPosition) > Math.Abs(bakPosition3 -
                        CurrentPosition))
                {
                    return bakPosition3;
                }
                return bakPosition1;
            }
        }

        private int GetMiddlePosition()
        {
            int middlePosition = int.MaxValue / 2;
            int actualItemCount = GetActualItemCountFromAdapter();
            if (actualItemCount > 0 && middlePosition % actualItemCount != 0)
            {
                middlePosition = middlePosition - middlePosition % actualItemCount;
            }
            return middlePosition;
        }
    }
}