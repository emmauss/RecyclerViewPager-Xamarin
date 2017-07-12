using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Support.V7.Widget;
using Java.Lang;

namespace Emmaus.Widget
{
    public class LoopRecyclerViewPagerAdapter : RecyclerViewPagerAdapter
    {

        //private static string TAG = LoopRecyclerViewPager.class.getSimpleName();
        private FieldInfo mPositionField;

        public LoopRecyclerViewPagerAdapter(RecyclerViewPager viewPager, RecyclerView.Adapter adapter)
            :base(viewPager,adapter)
        {
            
        }

        public int GetActualItemCount()
        {
            return base.ItemCount;
        }

        public int getActualItemViewType(int position)
        {
            return base.GetItemViewType(position);
        }

        public long getActualItemId(int position)
        {
            return base.GetItemId(position);
        }

        public override int ItemCount
        {
            get
            {
                if (GetActualItemCount() > 0)
                {
                    return int.MaxValue;
                }
                else
                {
                    return base.ItemCount;
                }
            }
        }

        public override int GetItemViewType(int position)
        {
            if (GetActualItemCount() > 0)
            {
                return base.GetItemViewType(GetActualPosition(position));
            }
            else
            {
                return 0;
            }
        }

        public override long GetItemId(int position)
        {
            return base.GetItemId(GetActualPosition(position));
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, GetActualPosition(position));
            // because of getCurrentPosition may return ViewHolderes position,
            // so we must reset mPosition if exists.
            //ViewHolderDelegate.setPosition(holder, position);
            if (mPositionField == null)
            {
                try
                {
                    mPositionField = holder.GetType().GetField("mPosition",BindingFlags.NonPublic 
                        | BindingFlags.Instance);
                }
                catch (NoSuchFieldException e)
                {
                    //Log.i(TAG, "The holder doesn't have a mPosition field.");
                }
            }
            if (mPositionField != null)
            {
                try
                {
                    mPositionField.SetValue(holder, position);
                }
                catch (Java.Lang.Exception e)
                {
                    //Log.w(TAG, "Error while updating holder's mPosition field", e);
                }
            }
        }

        public int GetActualPosition(int position)
        {
            int actualPosition = position;
            if (GetActualItemCount() > 0 && position >= GetActualItemCount())
            {
                actualPosition = position % GetActualItemCount();
            }
            return actualPosition;
        }
    }

    /*public abstract class ViewHolderDelegate
    {

        private ViewHolderDelegate()
        {
            throw new UnsupportedOperationException("no instances");
        }

        public static void setPosition(RecyclerView.ViewHolder viewHolder, int position)
        {
            viewHolder.AdapterPosition = position;
        }*/
    }