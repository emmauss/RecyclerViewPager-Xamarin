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
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Annotation;
using Android.Support.V7.Widget;
using Android.Support;
using Java.Lang;

namespace Emmaus.Widget
{
    /**
 * Implementation of {@link android.support.v4.view.PagerAdapter} that
 * uses a {@link android.support.v4.app.Fragment} to manage each page. This class also handles
 * saving and restoring of fragment's state.
 * <p/>
 * <p>This version of the pager is more useful when there are a large number
 * of pages, working more like a list view.  When pages are not visible to
 * the user, their entire fragment may be destroyed, only keeping the saved
 * state of that fragment.  This allows the pager to hold on to much less
 * memory associated with each visited page as compared to
 * {@link android.support.v4.app.FragmentPagerAdapter} at the cost of potentially more overhead when
 * switching between pages.
 * <p/>
 * <p>When using FragmentPagerAdapter the host ViewPager must have a
 * valid ID set.</p>
 * <p/>
 * <p>Subclasses only need to implement {@link #getItem(int, Fragment.SavedState)}
 * and {@link #getItemCount()} to have a working adapter.
 * <p>Warning:The fragment container id will be a simple sequence like [1,2,3....];
 * If you don't like this,you should use custom ContainerIdGenerator by  {@link #setContainerIdGenerator(IContainerIdGenerator)}
 * </p>
 */

    public abstract class FragmentStatePagerAdapter : RecyclerView.Adapter
    {
        private static readonly string TAG = "FragmentStatePagerAdapter";
        private static readonly bool DEBUG = false;

        private readonly Android.Support.V4.App.FragmentManager mFragmentManager;
        private Android.Support.V4.App.FragmentTransaction mCurTransaction = null;
        //private SparseArray<Fragment.SavedState> mStates = new SparseArray<>();
        private Android.Util.SparseArray<Android.Support.V4.App.Fragment.SavedState> mStates =
            new Android.Util.SparseArray<Android.Support.V4.App.Fragment.SavedState>();
        private HashSet<int> mIds = new HashSet<int>();
        private Random mRandom = new Random();
        private ContainerIdGenerator mContainerIdGenerator;

        public class ContainerIdGenerator : IContainerIdGenerator
        {
            Random mRandom;
            public ContainerIdGenerator(Random rand)
            {
                mRandom = rand;
            }
            public int GenId(HashSet<int> idContainer)
            {
                return System.Math.Abs(mRandom.Next());
            }
        }

        

        public FragmentStatePagerAdapter(Android.Support.V4.App.FragmentManager fm)
        {
            mFragmentManager = fm;
            mContainerIdGenerator = new ContainerIdGenerator(mRandom);
        }

        /**
     * set custom idGenerator
     */
        public void setContainerIdGenerator( ContainerIdGenerator idGenerator)
        {
            mContainerIdGenerator = idGenerator;
        }

        public override void OnViewRecycled(Java.Lang.Object holder)
        {

            if (mCurTransaction == null)
            {
                mCurTransaction = mFragmentManager.BeginTransaction();
            }
            if (DEBUG) Log.Verbose(TAG, "Removing item #");
            int tagId = GenTagId(((FragmentViewHolder)holder).AdapterPosition);
            Android.Support.V4.App.Fragment f = mFragmentManager.FindFragmentByTag(tagId.ToString());
            if (f != null)
            {
                if (DEBUG)
                    Log.Verbose(TAG, "Removing fragment #");
                mStates.Put(tagId, mFragmentManager.SaveFragmentInstanceState(f));
                mCurTransaction.Remove(f);
                mCurTransaction.CommitAllowingStateLoss();
                mCurTransaction = null;
                mFragmentManager.ExecutePendingTransactions();
            }
            if (((FragmentViewHolder)holder).ItemView is ViewGroup) {
                ((ViewGroup)((FragmentViewHolder)holder).ItemView).RemoveAllViews();
            }
            base.OnViewRecycled(holder);
        }

        

        public sealed override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.rvp_fragment_container, parent, false);
            int id = mContainerIdGenerator.GenId(mIds);
            if (parent.Context is Activity) {
                while (((Activity)parent.Context).Window.DecorView.FindViewById(id) != null)
                {
                    id = mContainerIdGenerator.GenId(mIds);
                }
            }
            view.FindViewById(Resource.Id.rvp_fragment_container).Id=id;
            mIds.Add(id);
            return new FragmentViewHolder(view, this);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            //throw new NotImplementedException();
        }

        public interface IContainerIdGenerator
        {
            int GenId(HashSet<int> idContainer);
        }

        protected int GenTagId(int position)
        {
            // itemId must not be zero
            long itemId = GetItemId(position);
            if (itemId == RecyclerView.NoId)
            {
                return position + 1;
            }
            else
            {
                return (int)itemId;
            }
        }

        /**
     * Return the Fragment associated with a specified position.
     */
        public abstract Android.Support.V4.App.Fragment GetItem(int position, 
            Android.Support.V4.App.Fragment.SavedState savedState);

        public abstract void OnDestroyItem(int position, Android.Support.V4.App.Fragment fragment);

        
        public class FragmentViewHolder : RecyclerView.ViewHolder, View.IOnAttachStateChangeListener
        {
            FragmentStatePagerAdapter mParent;
            public FragmentViewHolder(View itemView, FragmentStatePagerAdapter parent) : base(itemView)
            {
                mParent = parent;
                itemView.AddOnAttachStateChangeListener(this);

            }

            public void OnViewAttachedToWindow(View attachedView)
            {
                if (mParent.mCurTransaction == null)
                {
                    mParent.mCurTransaction = mParent.mFragmentManager.BeginTransaction();
                }
                int tagId = mParent.GenTagId(LayoutPosition);
                Android.Support.V4.App.Fragment fragmentInAdapter = mParent.GetItem(LayoutPosition,
                    mParent.mStates.Get(tagId));
                if (fragmentInAdapter != null)
                {
                    mParent.mCurTransaction.Replace(ItemView.Id, fragmentInAdapter, tagId + "");
                    mParent.mCurTransaction.CommitAllowingStateLoss();
                    mParent.mCurTransaction = null;
                    mParent.mFragmentManager.ExecutePendingTransactions();
                }
            }

            public void OnViewDetachedFromWindow(View detachedView)
            {
                if (DEBUG)
                    Log.Verbose(TAG, "Removing fragment #");
                int tagId = mParent.GenTagId(LayoutPosition);
                Android.Support.V4.App.Fragment frag = mParent.mFragmentManager.FindFragmentByTag(tagId + "");
                if (frag == null)
                {
                    return;
                }
                if (mParent.mCurTransaction == null)
                {
                    mParent.mCurTransaction = mParent.mFragmentManager.BeginTransaction();
                }
                mParent.mStates.Put(tagId, mParent.mFragmentManager.SaveFragmentInstanceState(frag));
                mParent.mCurTransaction.Remove(frag);
                mParent.mCurTransaction.CommitAllowingStateLoss();
                mParent.mCurTransaction = null;
                mParent.mFragmentManager.ExecutePendingTransactions();
                mParent.OnDestroyItem(LayoutPosition, frag);
            }
            
        }
    }

    

}