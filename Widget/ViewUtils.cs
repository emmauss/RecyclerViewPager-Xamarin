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

namespace Emmaus.Widget
{
    public class ViewUtils
    {
        /**
     * Get center child in X Axes
     */
        public static View GetCenterXChild(RecyclerView recyclerView)
        {
            int childCount = recyclerView.ChildCount;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    View child = recyclerView.GetChildAt(i);
                    if (IsChildInCenterX(recyclerView, child))
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        /**
         * Get position of center child in X Axes
         */
        public static int GetCenterXChildPosition(RecyclerView recyclerView)
        {
            int childCount = recyclerView.ChildCount;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    View child = recyclerView.GetChildAt(i);
                    if (IsChildInCenterX(recyclerView, child))
                    {
                        return recyclerView.GetChildAdapterPosition(child);
                    }
                }
            }
            return childCount;
        }

        /**
         * Get center child in Y Axes
         */
        public static View GetCenterYChild(RecyclerView recyclerView)
        {
            int childCount = recyclerView.ChildCount;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    View child = recyclerView.GetChildAt(i);
                    if (IsChildInCenterY(recyclerView, child))
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        /**
         * Get position of center child in Y Axes
         */
        public static int GetCenterYChildPosition(RecyclerView recyclerView)
        {
            int childCount = recyclerView.ChildCount;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    View child = recyclerView.GetChildAt(i);
                    if (IsChildInCenterY(recyclerView, child))
                    {
                        return recyclerView.GetChildAdapterPosition(child);
                    }
                }
            }
            return childCount;
        }

        public static bool IsChildInCenterX(RecyclerView recyclerView, View view)
        {
            int childCount = recyclerView.ChildCount;
            int[] lvLocationOnScreen = new int[2];
            int[] vLocationOnScreen = new int[2];
            recyclerView.GetLocationOnScreen(lvLocationOnScreen);
            int middleX = lvLocationOnScreen[0] + recyclerView.Width / 2;
            if (childCount > 0)
            {
                view.GetLocationOnScreen(vLocationOnScreen);
                if (vLocationOnScreen[0] <= middleX && vLocationOnScreen[0] + view.Width >= middleX)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsChildInCenterY(RecyclerView recyclerView, View view)
        {
            int childCount = recyclerView.ChildCount;
            int[] lvLocationOnScreen = new int[2];
            int[] vLocationOnScreen = new int[2];
            recyclerView.GetLocationOnScreen(lvLocationOnScreen);
            int middleY = lvLocationOnScreen[1] + recyclerView.Height / 2;
            if (childCount > 0)
            {
                view.GetLocationOnScreen(vLocationOnScreen);
                if (vLocationOnScreen[1] <= middleY && vLocationOnScreen[1] + view.Height >= middleY)
                {
                    return true;
                }
            }
            return false;
        }
    }
}