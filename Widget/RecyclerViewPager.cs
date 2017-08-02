using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Android.Support.V7;
using Android.Support.V7.Widget;

namespace Emmaus.Widget
{
    public class RecyclerViewPager : RecyclerView
    {
        public static bool DEBUG = Build.Tags.Contains("debug");


        private RecyclerViewPagerAdapter mViewPagerAdapter;
        private float mTriggerOffset = 0.25f;
        private float mFlingFactor = 0.15f;
        private float mMillisecondsPerInch = 25f;
        private float mTouchSpan;
        private List<OnPageChangedListener> mOnPageChangedListeners;
        private int mSmoothScrollTargetPosition = -1;
        private int mPositionBeforeScroll = -1;

        private bool mSinglePageFling;
        bool isInertia; // inertia slide state
        float minSlideDistance;
        PointF touchStartPoint;

       // RecyclerLinearSmoothScroller linearSmoothScroller; 
        bool mNeedAdjust;
        int mFisrtLeftWhenDragging;
        int mFirstTopWhenDragging;
        View mCurView;
        int mMaxLeftWhenDragging = int.MinValue;
        int mMinLeftWhenDragging = int.MinValue;
        int mMaxTopWhenDragging = int.MinValue;
        int mMinTopWhenDragging = int.MaxValue;
        private int mPositionOnTouchDown = -1;
        private bool mHasCalledOnPageChanged = true;
        private bool reverseLayout = false;
        private float mLastY;

        public RecyclerViewPager(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(context,null,0);
        }

        public RecyclerViewPager(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(context,attrs,0);
        }

        

        private void Initialize(Context context, IAttributeSet attrs, int defStyle)
        {
            initAttrs(context, attrs, defStyle);
          /* linearSmoothScroller =
                        new RecyclerLinearSmoothScroller(Context, this);*/
            NestedScrollingEnabled = false;
            ViewConfiguration viewConfiguration = ViewConfiguration.Get(context);
            minSlideDistance = viewConfiguration.ScaledTouchSlop;
        }

        private void initAttrs(Context context, IAttributeSet attrs, int defStyle)
        {
            TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.RecyclerViewPager, defStyle,
                    0);
            mFlingFactor = a.GetFloat(Resource.Styleable.RecyclerViewPager_rvp_flingFactor, 0.15f);
            mTriggerOffset = a.GetFloat(Resource.Styleable.RecyclerViewPager_rvp_triggerOffset, 0.25f);
            mSinglePageFling = a.GetBoolean(Resource.Styleable.RecyclerViewPager_rvp_singlePageFling, mSinglePageFling);
            isInertia = a.GetBoolean(Resource.Styleable.RecyclerViewPager_rvp_inertia, false);
            mMillisecondsPerInch = a.GetFloat(Resource.Styleable.RecyclerViewPager_rvp_millisecondsPerInch, 25f);
            a.Recycle();
        }


        public float TriggerOffset
        {
            set
            {
                mTriggerOffset = value;
            }
            get
            {
                return mTriggerOffset;
            }
        }


        public bool IsSinglePageFling
        {
            set
            {
                mSinglePageFling = value;
            }
            get
            {
                return mSinglePageFling;
            }
        }


        public bool IsInertia
        {
            set
            {
                isInertia = value;
            }
            get
            {
                return isInertia;
            }
        }

        public float FlingFactor
        {
            set
            {
                mFlingFactor = value;
            }
            get
            {
                return mFlingFactor;
            }
        }


        protected override void OnRestoreInstanceState(IParcelable state)
        {


            try
            {
                FieldInfo fLayoutState = state.GetType().GetField("mLayoutState");
                //fLayoutState.(true);
                Object layoutState = fLayoutState.GetValue(state);
                FieldInfo fAnchorOffset = layoutState.GetType().GetField("mAnchorOffset");
                FieldInfo fAnchorPosition = layoutState.GetType().GetField("mAnchorPosition");
                /*fAnchorPosition.setAccessible(true);
                fAnchorOffset.setAccessible(true);*/
                if ((int)fAnchorOffset.GetValue(layoutState) > 0)
                {
                    fAnchorPosition.SetValue(layoutState, (int)fAnchorPosition.GetValue(layoutState) - 1);
                }
                else if ((int)fAnchorOffset.GetValue(layoutState) < 0)
                {
                    fAnchorPosition.SetValue(layoutState, (int)fAnchorPosition.GetValue(layoutState) + 1);
                }
                fAnchorOffset.SetValue(layoutState, 0);
            }
            catch (Exception e)
            {
                // e.printStackTrace();
            }
            base.OnRestoreInstanceState(state);
        }



        public override void SetAdapter(Adapter adapter)
        {

            mViewPagerAdapter = EnsureRecyclerViewPagerAdapter(adapter);
            base.SetAdapter(mViewPagerAdapter);
        }

        public override void SwapAdapter(Adapter adapter, bool removeAndRecycleExistingViews)
        {
            mViewPagerAdapter = EnsureRecyclerViewPagerAdapter(adapter);
            base.SwapAdapter(mViewPagerAdapter, removeAndRecycleExistingViews);
        }

        public override Adapter GetAdapter()
        {
            if (mViewPagerAdapter != null)
            {
                return mViewPagerAdapter.mAdapter;
            }
            return base.GetAdapter();
        }

        public RecyclerViewPagerAdapter GetWrapperAdapter()
        {
            return mViewPagerAdapter;
        }

        public override void SetLayoutManager(LayoutManager layout)
        {
            base.SetLayoutManager(layout);

            if (layout is LinearLayoutManager)
            {
                reverseLayout = ((LinearLayoutManager)layout).ReverseLayout;
            }
        }

        public override bool Fling(int velocityX, int velocityY)
        {
            //return base.Fling(velocityX, velocityY);

            bool flinging = base.Fling((int)(velocityX * mFlingFactor), (int)(velocityY * mFlingFactor));
            if (flinging)
            {
                if (GetLayoutManager().CanScrollHorizontally())
                {
                    adjustPositionX(velocityX);
                }
                else
                {
                    AdjustPositionY(velocityY);
                }
            }

            if (DEBUG)
            {
                Log.Debug("@", "velocityX:" + velocityX);
                Log.Debug("@", "velocityY:" + velocityY);
            }
            return flinging;
        }

        public override void SmoothScrollToPosition(int position)
        {
            if (mPositionBeforeScroll < 0)
            {
                mPositionBeforeScroll = CurrentPosition;
            }
            mSmoothScrollTargetPosition = position;
            if (GetLayoutManager() != null && GetLayoutManager() is LinearLayoutManager)
            {
                // exclude item decoration
             var linearSmoothScroller =
                        new RecyclerLinearSmoothScroller(Context);
                linearSmoothScroller.SetParent(this);

                linearSmoothScroller.TargetPosition = position;
                if (position == RecyclerView.NoPosition)
                {
                    return;
                }
                GetLayoutManager().StartSmoothScroll(linearSmoothScroller);
            }
            else
            {
                base.SmoothScrollToPosition(position);
            }

        }



        public class RecyclerLinearSmoothScroller : LinearSmoothScroller
        {
            RecyclerViewPager mParent;
            public RecyclerLinearSmoothScroller(Context context) : base(context)
            {
                
            }

            public void SetParent(RecyclerViewPager parent)
            {
                mParent = parent;
            }
            public override PointF ComputeScrollVectorForPosition(int targetPosition)
            {
                int currentPosition = mParent.mPositionBeforeScroll;
                if (currentPosition < targetPosition)
                {
                    return new PointF(1, 0);
                }
                else
                {
                    return new PointF(-1, 0);
                }
            }

            
            protected override void OnTargetFound(View targetView, State state, Action action)
            {
                if (LayoutManager == null)
                {
                    return;
                }
                int dx = CalculateDxToMakeVisible(targetView,
                        HorizontalSnapPreference);
                int dy = CalculateDyToMakeVisible(targetView,
                        VerticalSnapPreference);
                if (dx > 0)
                {
                    dx = dx - LayoutManager
                            .GetLeftDecorationWidth(targetView);
                }
                else
                {
                    dx = dx + LayoutManager
                            .GetRightDecorationWidth(targetView);
                }
                if (dy > 0)
                {
                    dy = dy - LayoutManager
                            .GetTopDecorationHeight(targetView);
                }
                else
                {
                    dy = dy + LayoutManager
                            .GetBottomDecorationHeight(targetView);
                }
                int distance = (int)Math.Sqrt(dx * dx + dy * dy);
                int time = CalculateTimeForDeceleration(distance);
                if (time > 0)
                {
                    action.Update(-dx, -dy, time, MDecelerateInterpolator);
                }
            }

            protected override float CalculateSpeedPerPixel(DisplayMetrics displayMetrics)
            {   
                if(mParent != null)
                return mParent.mMillisecondsPerInch / (float)displayMetrics.DensityDpi;

                return base.CalculateSpeedPerPixel(displayMetrics);

            }

            protected override void OnStop()
            {
                base.OnStop();
                if (mParent.mOnPageChangedListeners != null)
                {
                    foreach (OnPageChangedListener onPageChangedListener in mParent.mOnPageChangedListeners)
                    {
                        if (onPageChangedListener != null)
                        {
                            onPageChangedListener.OnPageChanged(mParent.mPositionBeforeScroll,
                                mParent.mSmoothScrollTargetPosition);
                        }
                    }
                }
                mParent.mHasCalledOnPageChanged = true;
            }
        }

        public override void ScrollToPosition(int position)
        {

            base.ScrollToPosition(position);
        }

        public class RecyclerViewTreeObserverListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
        {
            RecyclerViewPager mParent;
            public RecyclerViewTreeObserverListener(RecyclerViewPager parent)
            {
                mParent = parent;
            }

            public void OnGlobalLayout()
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
                {
                    mParent.ViewTreeObserver.RemoveGlobalOnLayoutListener(this);
                }
                else
                {
                    mParent.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
                }

                if (mParent.mSmoothScrollTargetPosition >= 0 && mParent.mSmoothScrollTargetPosition < mParent.ItemCount)
                {
                    if (mParent.mOnPageChangedListeners != null)
                    {
                        foreach (OnPageChangedListener onPageChangedListener in mParent.mOnPageChangedListeners)
                        {
                            if (onPageChangedListener != null)
                            {
                                onPageChangedListener.OnPageChanged(mParent.mPositionBeforeScroll,
                                    mParent.CurrentPosition);
                            }
                        }
                    }
                }
            }
        }

        public int ItemCount
        {
            get
            {
                return mViewPagerAdapter == null ? 0 : mViewPagerAdapter.ItemCount;
            }
        }

        /**
     * get item position in center of viewpager
     */
        public int CurrentPosition
        {
            get
            {
                int curPosition;
                if (GetLayoutManager().CanScrollHorizontally())
                {
                    curPosition = ViewUtils.GetCenterXChildPosition(this);
                }
                else
                {
                    curPosition = ViewUtils.GetCenterYChildPosition(this);
                }
                if (curPosition < 0)
                {
                    curPosition = mSmoothScrollTargetPosition;
                }
                return curPosition;
            }
        }

        protected void adjustPositionX(int velocityX)
        {
            if (reverseLayout) velocityX *= -1;

            int childCount = ChildCount;
            if (childCount > 0)
            {
                int curPosition = ViewUtils.GetCenterXChildPosition(this);
                int childWidth = Width - PaddingLeft - PaddingRight;
                int flingCount = getFlingCount(velocityX, childWidth);
                int targetPosition = curPosition + flingCount;
                if (mSinglePageFling)
                {
                    flingCount = Math.Max(-1, Math.Min(1, flingCount));
                    targetPosition = flingCount == 0 ? curPosition : mPositionOnTouchDown + flingCount;
                    if (DEBUG)
                    {
                        Log.Debug("@", "flingCount:" + flingCount);
                        Log.Debug("@", "original targetPosition:" + targetPosition);
                    }
                }
                targetPosition = Math.Max(targetPosition, 0);
                targetPosition = Math.Min(targetPosition, ItemCount - 1);
                if (targetPosition == curPosition
                        && (!mSinglePageFling || mPositionOnTouchDown == curPosition))
                {
                    View centerXChild = ViewUtils.GetCenterXChild(this);
                    if (centerXChild != null)
                    {
                        if (mTouchSpan > centerXChild.Width * mTriggerOffset * mTriggerOffset && targetPosition != 0)
                        {
                            if (!reverseLayout) targetPosition--;
                            else targetPosition++;
                        }
                        else if (mTouchSpan < centerXChild.Width * -mTriggerOffset && targetPosition != ItemCount - 1)
                        {
                            if (!reverseLayout) targetPosition++;
                            else targetPosition--;
                        }
                    }
                }
                if (DEBUG)
                {
                    Log.Debug("@", "mTouchSpan:" + mTouchSpan);
                    Log.Debug("@", "adjustPositionX:" + targetPosition);
                }
                SmoothScrollToPosition(safeTargetPosition(targetPosition, ItemCount));
            }
        }

        public void AddOnPageChangedListener(OnPageChangedListener listener)
        {
            if (mOnPageChangedListeners == null)
            {
                mOnPageChangedListeners = new List<OnPageChangedListener>();
            }
            mOnPageChangedListeners.Add(listener);
        }

        public void RemoveOnPageChangedListener(OnPageChangedListener listener)
        {
            if (mOnPageChangedListeners != null)
            {
                mOnPageChangedListeners.Remove(listener);
            }
        }

        public void ClearOnPageChangedListeners()
        {
            if (mOnPageChangedListeners != null)
            {
                mOnPageChangedListeners.Clear();
            }
        }

        /***
         * adjust position before Touch event complete and fling action start.
         */
        protected void AdjustPositionY(int velocityY)
        {
            if (reverseLayout) velocityY *= -1;

            int childCount = ChildCount;
            if (childCount > 0)
            {
                int curPosition = ViewUtils.GetCenterYChildPosition(this);
                int childHeight = Height - PaddingTop - PaddingBottom;
                int flingCount = getFlingCount(velocityY, childHeight);
                int targetPosition = curPosition + flingCount;
                if (mSinglePageFling)
                {
                    flingCount = Math.Max(-1, Math.Min(1, flingCount));
                    targetPosition = flingCount == 0 ? curPosition : mPositionOnTouchDown + flingCount;
                }

                targetPosition = Math.Max(targetPosition, 0);
                targetPosition = Math.Min(targetPosition, ItemCount - 1);
                if (targetPosition == curPosition
                        && (!mSinglePageFling || mPositionOnTouchDown == curPosition))
                {
                    View centerYChild = ViewUtils.GetCenterYChild(this);
                    if (centerYChild != null)
                    {
                        if (mTouchSpan > centerYChild.Height * mTriggerOffset && targetPosition != 0)
                        {
                            if (!reverseLayout) targetPosition--;
                            else targetPosition++;
                        }
                        else if (mTouchSpan < centerYChild.Height * -mTriggerOffset
                            && targetPosition != ItemCount - 1)
                        {
                            if (!reverseLayout) targetPosition++;
                            else targetPosition--;
                        }
                    }
                }
                if (DEBUG)
                {
                    Log.Debug("@", "mTouchSpan:" + mTouchSpan);
                    Log.Debug("@", "AdjustPositionY:" + targetPosition);
                }
                SmoothScrollToPosition(safeTargetPosition(targetPosition, ItemCount));
            }
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down && GetLayoutManager() != null)
            {
                mPositionOnTouchDown = GetLayoutManager().CanScrollHorizontally()
                        ? ViewUtils.GetCenterXChildPosition(this)
                        : ViewUtils.GetCenterYChildPosition(this);
                if (DEBUG)
                {
                    Log.Debug("@", "mPositionOnTouchDown:" + mPositionOnTouchDown);
                }
                mLastY = e.RawY;
            }
            return base.DispatchTouchEvent(e);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            // recording the max/min value in touch track
            if (e.Action == MotionEventActions.Move)
            {
                if (mCurView != null)
                {
                    mMaxLeftWhenDragging = Math.Max(mCurView.Left, mMaxLeftWhenDragging);
                    mMaxTopWhenDragging = Math.Max(mCurView.Top, mMaxTopWhenDragging);
                    mMinLeftWhenDragging = Math.Min(mCurView.Left, mMinLeftWhenDragging);
                    mMinTopWhenDragging = Math.Min(mCurView.Top, mMinTopWhenDragging);
                }
            }
            return base.OnTouchEvent(e);
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (isInertia)
            {
                float x = ev.RawX;
                float y = ev.RawY;
                if (touchStartPoint == null)
                    touchStartPoint = new PointF();
                switch (MotionEventActions.Mask & ev.Action)
                {
                    case MotionEventActions.Down:
                        touchStartPoint.Set(x, y);
                        break;
                    case MotionEventActions.Move:
                        float tempDistance = (float)Math.Sqrt(x * x + y * y);
                        float lastDistance = (float)Math.Sqrt(touchStartPoint.X * touchStartPoint.Y
                            + touchStartPoint.Y * touchStartPoint.Y);

                        if (Math.Abs(lastDistance - tempDistance) > minSlideDistance)
                        {
                            float k = Math.Abs((touchStartPoint.X - y) / (touchStartPoint.X - x));
                            // prevent tan 90Åã calc
                            if (Math.Abs(touchStartPoint.Y - y) < 1)
                                return GetLayoutManager().CanScrollHorizontally();
                            if (Math.Abs(touchStartPoint.X - x) < 1)
                                return !GetLayoutManager().CanScrollHorizontally();
                            return k < Math.Tan(ToRadian(30F));
                        }
                        break;

                        double ToRadian(double angle)
                        {
                            return Math.PI * angle / 180.0;
                        }
                }
            }
            return base.OnInterceptTouchEvent(ev);
        }

        public override void OnScrollStateChanged(int state)
        {
            base.OnScrollStateChanged(state);

            if (state == ScrollStateDragging)
            {
                mNeedAdjust = true;
                mCurView = GetLayoutManager().CanScrollHorizontally() ? ViewUtils.GetCenterXChild(this) :
                        ViewUtils.GetCenterYChild(this);
                if (mCurView != null)
                {
                    if (mHasCalledOnPageChanged)
                    {
                        // While rvp is scrolling, mPositionBeforeScroll will be previous value.
                        mPositionBeforeScroll = GetChildLayoutPosition(mCurView);
                        mHasCalledOnPageChanged = false;
                    }
                    if (DEBUG)
                    {
                        Log.Debug("@", "mPositionBeforeScroll:" + mPositionBeforeScroll);
                    }
                    mFisrtLeftWhenDragging = mCurView.Left;
                    mFirstTopWhenDragging = mCurView.Top;
                }
                else
                {
                    mPositionBeforeScroll = -1;
                }
                mTouchSpan = 0;
            }
            else if (state == ScrollStateSettling)
            {
                mNeedAdjust = false;
                if (mCurView != null)
                {
                    if (GetLayoutManager().CanScrollHorizontally())
                    {
                        mTouchSpan = mCurView.Left - mFisrtLeftWhenDragging;
                    }
                    else
                    {
                        mTouchSpan = mCurView.Top - mFirstTopWhenDragging;
                    }
                }
                else
                {
                    mTouchSpan = 0;
                }
                mCurView = null;
            }
            else if (state == ScrollStateIdle)
            {
                if (mNeedAdjust)
                {
                    int targetPosition = GetLayoutManager().CanScrollHorizontally() ?
                        ViewUtils.GetCenterXChildPosition(this) :
                            ViewUtils.GetCenterYChildPosition(this);
                    if (mCurView != null)
                    {
                        targetPosition = GetChildAdapterPosition(mCurView);
                        if (GetLayoutManager().CanScrollHorizontally())
                        {
                            int spanX = mCurView.Left - mFisrtLeftWhenDragging;
                            // if user is tending to cancel paging action, don't perform position changing
                            if (spanX > mCurView.Width * mTriggerOffset && mCurView.Left >= mMaxLeftWhenDragging)
                            {
                                if (!reverseLayout) targetPosition--;
                                else targetPosition++;
                            }
                            else if (spanX < mCurView.Width * -mTriggerOffset && mCurView.Left <= mMinLeftWhenDragging)
                            {
                                if (!reverseLayout) targetPosition++;
                                else targetPosition--;
                            }
                        }
                        else
                        {
                            int spanY = mCurView.Top - mFirstTopWhenDragging;
                            if (spanY > mCurView.Height * mTriggerOffset && mCurView.Top >= mMaxTopWhenDragging)
                            {
                                if (!reverseLayout) targetPosition--;
                                else targetPosition++;
                            }
                            else if (spanY < mCurView.Height * -mTriggerOffset && mCurView.Top <= mMinTopWhenDragging)
                            {
                                if (!reverseLayout) targetPosition++;
                                else targetPosition--;
                            }
                        }
                    }
                    SmoothScrollToPosition(safeTargetPosition(targetPosition, ItemCount));
                    mCurView = null;
                }
                else if (mSmoothScrollTargetPosition != mPositionBeforeScroll)
                {
                    if (DEBUG)
                    {
                        Log.Debug("@", "onPageChanged:" + mSmoothScrollTargetPosition);
                    }
                    mPositionBeforeScroll = mSmoothScrollTargetPosition;
                }
                // reset
                mMaxLeftWhenDragging = int.MinValue;
                mMinLeftWhenDragging = int.MaxValue;
                mMaxTopWhenDragging = int.MinValue;
                mMinTopWhenDragging = int.MaxValue;
            }
        }

        public virtual RecyclerViewPagerAdapter EnsureRecyclerViewPagerAdapter(Adapter adapter)
        {
            return (adapter is RecyclerViewPagerAdapter)
                ? (RecyclerViewPagerAdapter)adapter
                : new RecyclerViewPagerAdapter(this, adapter);

        }

        private int getFlingCount(int velocity, int cellSize)
        {
            if (velocity == 0)
            {
                return 0;
            }
            int sign = velocity > 0 ? 1 : -1;
            return (int)(sign * Math.Ceiling((velocity * sign * mFlingFactor / cellSize)
                    - mTriggerOffset));
        }

        private int safeTargetPosition(int position, int count)
        {
            if (position < 0)
            {
                return 0;
            }
            if (position >= count)
            {
                return count - 1;
            }
            return position;
        }

        public interface OnPageChangedListener
        {
            /**
             * Fires when viewpager changes it's page
             * @param oldPosition old position
             * @param newPosition new position
             */
            void OnPageChanged(int oldPosition, int newPosition);
        }


        public float getlLastY()
        {
            return mLastY;
        }
    }
}