using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;

namespace TabPageViewController_Xamarin
{
    public partial class TabView : UIView
        , IUICollectionViewDataSource
        , IUICollectionViewDelegate
        , IUICollectionViewDelegateFlowLayout
    {
        public Action<nint, UIPageViewControllerNavigationDirection> pageItemPressedBlock;
        string[] _pageTabItems;
        public string[] pageTabItems
        {
            get { return _pageTabItems; }
            set
            {
                _pageTabItems = value;
                pageTabItemsCount = pageTabItems.Length;
                beforeIndex = pageTabItems.Length;
            }
        }
        public bool isInfinity { get; set; }
        public bool layouted { get; set; } = false;
        public TabPageOption option { get; set; }
        public int beforeIndex { get; set; }
        public int currentIndex { get; set; }
        public int pageTabItemsCount { get; set; }
        public bool shouldScrollToItem { get; set; }
        public nfloat pageTabItemsWidth { get; set; }
        public nfloat collectionViewContentOffsetX { get; set; }
        public nfloat currentBarViewWidth { get; set; }
        public TabCollectionCell cellForSize { get; set; }
        public Dictionary<NSIndexPath, CGSize> cachedCellSizes { get; set; }
        public NSLayoutConstraint currentBarViewLeftConstraint { get; set; }

        public TabView(NSCoder coder) : base(coder)
        {

        }

        public TabView(bool _isInfinity, TabPageOption _option) : base(CGRect.Empty)
        {
            option = _option;
            isInfinity = _isInfinity;
            NSBundle.FromClass(new ObjCRuntime.Class(typeof(TabView))).LoadNib(nameof(TabView), this, null);
            AddSubview(contentView);
            contentView.BackgroundColor = option.tabBackgroundColor.ColorWithAlpha(option.tabBarAlpha);

            var top = NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, 0.0f);
            var left = NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1.0f, 0.0f);
            var bottom = NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, contentView, NSLayoutAttribute.Bottom, 1.0f, 0.0f);
            var right = NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, contentView, NSLayoutAttribute.Trailing, 1.0f, 0.0f);
            contentView.TranslatesAutoresizingMaskIntoConstraints = false;
            this.AddConstraints(new NSLayoutConstraint[] { top, left, bottom, right });

            var bundle = NSBundle.FromClass(new ObjCRuntime.Class(typeof(TabView)));
            var nib = UINib.FromName(TabCollectionCell.cellIdentifier(), bundle);
            collectionView.RegisterNibForCell(nib, TabCollectionCell.cellIdentifier());
            cellForSize = nib.Instantiate(null, null).First() as TabCollectionCell;

            collectionView.ScrollsToTop = false;
            collectionView.DataSource = this;
            collectionView.Delegate = this;
            //collectionView.SetCollectionViewLayout(this, true);
            collectionView.Delegate = this;

            currentBarView.BackgroundColor = option.currentColor;
            currentBarViewHeightConstraint.Constant = option.currentBarHeight;

            if (!isInfinity)
            {
                currentBarView.RemoveFromSuperview();
                collectionView.AddSubview(currentBarView);
                collectionView.TranslatesAutoresizingMaskIntoConstraints = false;

                var topOne = NSLayoutConstraint.Create(currentBarView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, collectionView, NSLayoutAttribute.Top, 1.0f,
                    option.tabHeight - currentBarViewHeightConstraint.Constant);
                var leftOne = NSLayoutConstraint.Create(currentBarView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, collectionView, NSLayoutAttribute.Leading, 1.0f, 0.0f);

                currentBarViewLeftConstraint = leftOne;
                collectionView.AddConstraints(new[] { topOne, leftOne });
            }

            bottomBarViewHeightConstraint.Constant = 1.0f / UIScreen.MainScreen.Scale;
        }

        public void scrollCurrentBarView(int index, nfloat contentOffsetX)
        {
            var nextIndex = isInfinity ? index + pageTabItemsCount : index;
            if(isInfinity && index == 0 && (beforeIndex - pageTabItemsCount) == pageTabItemsCount - 1)
            {
                nextIndex = pageTabItemsCount * 2;
            }
            else if (isInfinity && (index == pageTabItemsCount - 1) && (beforeIndex - pageTabItemsCount) == 0)
            {
                nextIndex = pageTabItemsCount - 1;
            }

            if(collectionViewContentOffsetX ==  0.0)
            {
                collectionViewContentOffsetX = collectionView.ContentOffset.X;
            }

            var currentIndexPath = NSIndexPath.FromItemSection(currentIndex, 0);
            var nextIndexPath = NSIndexPath.FromItemSection(nextIndex, 0);
            var currentCell = collectionView.CellForItem(currentIndexPath) as TabCollectionCell;
            var nextCell = collectionView.CellForItem(nextIndexPath) as TabCollectionCell;

            nextCell.hideCurrentBarView();
            currentCell.hideCurrentBarView();
            currentBarView.Hidden = false;

            if(currentBarViewWidth == 0.0)
            {
                currentBarViewWidth = currentCell.Frame.Width;
            }

            var distance = (currentCell.Frame.Width / 2.0) + (nextCell.Frame.Width / 2.0);
            var scrollRate = contentOffsetX / Frame.Width;

            if(Math.Abs(scrollRate) > 0.6)
            {
                nextCell.highlightTitle();
                currentCell.unHighlightTitle();
            }
            else
            {
                nextCell.unHighlightTitle();
                currentCell.highlightTitle();
            }

            var width = (nfloat)Math.Abs(scrollRate) * (nextCell.Frame.Width - currentCell.Frame.Width);

            if (isInfinity)
            {
                var scroll = scrollRate * distance;
                collectionView.SetContentOffset(new CGPoint(collectionViewContentOffsetX + scroll, collectionView.ContentOffset.Y), true);
            }
            else
            {
                if(scrollRate > 0)
                {
                    currentBarViewLeftConstraint.Constant = currentCell.Frame.GetMinX() + scrollRate * currentCell.Frame.Width;
                }
                else
                {
                    currentBarViewLeftConstraint.Constant = currentCell.Frame.GetMinX() + nextCell.Frame.Width * scrollRate;
                }
            }

            currentBarViewWidthConstraint.Constant = currentBarViewWidth + width;
        }


        /**
        Center the current cell after page swipe
        */
        public void scrollToHorizontalCenter()
        {
            var indexPath = NSIndexPath.Create(currentIndex, 0);
            collectionView.ScrollToItem(indexPath, UICollectionViewScrollPosition.CenteredHorizontally, false);
            collectionViewContentOffsetX = collectionView.ContentOffset.X;
        }

        /**
        Called in after the transition is complete pages in isInfinityTabPageViewController in the process of updating the current 
        - parameter index: Next Index
        */
        public void updateCurrentIndex(int index, bool shouldScroll)
        {
            deselectVisibleCells();

            currentIndex = isInfinity ? index + pageTabItemsCount : index;

            var indexPath = NSIndexPath.Create(currentIndex, 0);
            moveCurrentBarView(indexPath, animated: !isInfinity, shouldScroll: shouldScroll);
        }

        /**
     Make the tapped cell the current if isInfinity is true

     - parameter index: Next IndexPath√
     */
        public void updateCurrentIndexForTap(int index)
        {
            deselectVisibleCells();

            if (isInfinity && (index < pageTabItemsCount) || (index >= pageTabItemsCount * 2))
            {
                currentIndex = (index < pageTabItemsCount) ? index + pageTabItemsCount : index - pageTabItemsCount;
                shouldScrollToItem = true;
            }
            else
            {
                currentIndex = index;
            }
            var indexPath = NSIndexPath.Create(index, 0);
            moveCurrentBarView(indexPath, animated: true, shouldScroll: true);
        }

        /**
     Move the collectionView to IndexPath of Current

     - parameter indexPath: Next IndexPath
     - parameter animated: true when you tap to move the isInfinityTabCollectionCell
     - parameter shouldScroll:
     */
        private void moveCurrentBarView(NSIndexPath indexPath, bool animated, bool shouldScroll)
        {
            if (shouldScroll)
            {
                collectionView.ScrollToItem(indexPath, UICollectionViewScrollPosition.CenteredHorizontally,animated);
                LayoutIfNeeded();
                collectionViewContentOffsetX = 0.0f;
                currentBarViewWidth = 0.0f;
            }

            var cell = collectionView.CellForItem(indexPath);

            if (cell is TabCollectionCell currentCell)
            {
                currentBarView.Hidden = false;
                if (animated && shouldScroll)
                {
                    currentCell.isCurrent = true;
                }
                currentCell.hideCurrentBarView();
                currentBarViewWidthConstraint.Constant = cell.Frame.Width;
                if (!isInfinity)
                {
                    currentBarViewLeftConstraint.Constant = cell.Frame.Location.X;
                }
                UIView.Animate(0.2, animation: () =>
                {
                    LayoutIfNeeded();

                },
                completion: () =>
                {
                    if (!animated && shouldScroll)
                    {
                        currentCell.isCurrent = true;
                    }
                    this.updateCollectionViewUserInteractionEnabled(true);
                });
                //    UIView.animate(withDuration: 0.2, animations: {
                //        self.layoutIfNeeded()
                //        }, completion:
                //    {
                //        _ in
                //        if !animated && shouldScroll {
                //            cell.isCurrent = true
                //        }

                //        self.updateCollectionViewUserInteractionEnabled(true)
                //})
            }
            beforeIndex = currentIndex;
        }

        /**
    Touch event control of collectionView

    - parameter userInteractionEnabled: collectionViewに渡すuserInteractionEnabled
    */
        public void updateCollectionViewUserInteractionEnabled(bool userInteractionEnabled)
        {
            collectionView.UserInteractionEnabled = userInteractionEnabled;
        }

        /**
     Update all of the cells in the display to the unselected state
     */
        private void deselectVisibleCells()
        {
            //collectionView.visibleCells.flatMap { $0 as? TabCollectionCell }.forEach { $0.isCurrent = false }
        }

        #region IUICollectionViewDataSource
        [Export("collectionView:numberOfItemsInSection:")]
        public nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return isInfinity ? pageTabItemsCount * 3 : pageTabItemsCount;
        }

        [Export("collectionView:cellForItemAtIndexPath:")]
        public UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell(TabCollectionCell.cellIdentifier(), indexPath) as TabCollectionCell;
            configureCell(cell, indexPath: indexPath);
            return cell;
        }

        public void configureCell(TabCollectionCell cell, NSIndexPath indexPath)
        {
            var fixedIndex = isInfinity ? indexPath.Item % pageTabItemsCount : indexPath.Item;
            cell.item = pageTabItems[fixedIndex];
            cell.option = option;
            cell.isCurrent = fixedIndex == (currentIndex % pageTabItemsCount);

            cell.tabItemButtonPressedBlock = (arg1, arg2) =>
            {
                var direction = UIPageViewControllerNavigationDirection.Forward;
                var pageTabItemsCount = arg1.pageTabItemsCount;
                var currentIndex = arg1.currentIndex;
                if (arg1.isInfinity)
                {
                    if (indexPath.Item < pageTabItemsCount || indexPath.Item < currentIndex)
                    {
                        direction = UIPageViewControllerNavigationDirection.Reverse;
                    }
                }
                else
                {
                    if (indexPath.Item < currentIndex)
                    {
                        direction = UIPageViewControllerNavigationDirection.Reverse;
                    }
                }

                arg1.pageItemPressedBlock(fixedIndex, direction);

                if (arg2?.isCurrent == false)
                {
                    arg1.updateCollectionViewUserInteractionEnabled(false);
                }

                arg1.updateCurrentIndexForTap((int)indexPath.Item);
            };
        }


        public void WillDisplayCell(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
        {
            var currentcell = cell as TabCollectionCell;
            if (currentcell != null && layouted)
            {
                var fixedIndex = isInfinity ? indexPath.Item % pageTabItemsCount : indexPath.Item;
                currentcell.isCurrent = fixedIndex == (currentIndex % pageTabItemsCount);
            }
        }

        #endregion

        #region IUICollectionViewDelegate

        public void scrollViewDidScroll(UIScrollView scrollView)
        {
            if (scrollView.Dragging)
            {
                currentBarView.Hidden = true;
                var indexPath = NSIndexPath.FromItemSection(currentIndex, 0);
                var cell = collectionView.CellForItem(indexPath) as TabCollectionCell;
                if(!(cell is null))
                {
                    cell.showCurrentBarView();
                }
            }

            if (!isInfinity) return;

            if(pageTabItemsWidth == 0.0f)
            {
                pageTabItemsWidth = (nfloat)Math.Floor(scrollView.ContentSize.Width / 3.0);
            }

            if(scrollView.ContentOffset.X <= 0.0 || scrollView.ContentOffset.X > pageTabItemsWidth * 2.0)
            {
                scrollView.SetContentOffset(new CGPoint(pageTabItemsWidth, scrollView.ContentOffset.Y), true);
            }
        }

        public void scrollViewDidEndScrollingAnimation(UIScrollView scrollView)
        {
            updateCollectionViewUserInteractionEnabled(true);

            if (!isInfinity) return;

            var indexPath = NSIndexPath.FromItemSection(currentIndex, 0);
            if (shouldScrollToItem)
            {
                collectionView.ScrollToItem(indexPath, UICollectionViewScrollPosition.CenteredHorizontally, false);
                shouldScrollToItem = false;
            }
        }

        #endregion

        #region IUICollectionViewDelegateFlowLayout
        [Export("collectionView:layout:sizeForItemAtIndexPath:")]
        public CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout collectionViewLayout, NSIndexPath indexPath)
        {
            var size = cachedCellSizes[indexPath];
            if (size != CGSize.Empty) return size;

            configureCell(cellForSize, indexPath);

            var newSize = cellForSize.SizeThatFits(new CGSize(collectionView.Bounds.Width, option.tabHeight));
            cachedCellSizes[indexPath] = newSize;

            return newSize;
        }
        [Export("collectionView:layout:minimumItemSpacingForSectionAtIndex:")]
        public nfloat GetMinimumInteritemSpacingForSection(UICollectionView collectionView, UICollectionViewLayout collectionViewLayout, int section)
        {
            return 0.0f;
        }
        [Export("collectionView:layout:minimumLineSpacingForSectionAtIndex:")]
        public nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView, UICollectionViewLayout collectionViewLayout, int section)
        {
            return 0.0f;
        }
        #endregion
    }

    public class TabViewCollectionDataSource : UICollectionViewSource
    {
        TabView tabView;
        public TabViewCollectionDataSource(TabView _tabView)
        {
            tabView = _tabView;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell(TabCollectionCell.cellIdentifier(), indexPath) as TabCollectionCell;
            configureCell(cell, indexPath: indexPath, tabView);
            return cell;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return tabView.isInfinity ? tabView.pageTabItemsCount * 3 : tabView.pageTabItemsCount;
        }

        public void configureCell(TabCollectionCell cell, NSIndexPath indexPath, TabView tabView)
        {
            var fixedIndex = tabView.isInfinity ? indexPath.Item % tabView.pageTabItemsCount : indexPath.Item;
            cell.item = tabView.pageTabItems[fixedIndex];
            cell.option = tabView.option;
            cell.isCurrent = fixedIndex == (tabView.currentIndex % tabView.pageTabItemsCount);

            cell.tabItemButtonPressedBlock = (arg1, arg2) =>
            {
                var direction = UIPageViewControllerNavigationDirection.Forward;
                var pageTabItemsCount = arg1.pageTabItemsCount;
                var currentIndex = arg1.currentIndex;
                if (arg1.isInfinity)
                {
                    if (indexPath.Item < pageTabItemsCount || indexPath.Item < currentIndex)
                    {
                        direction = UIPageViewControllerNavigationDirection.Reverse;
                    }
                }
                else
                {
                    if (indexPath.Item < currentIndex)
                    {
                        direction = UIPageViewControllerNavigationDirection.Reverse;
                    }
                }

                arg1.pageItemPressedBlock(fixedIndex, direction);

                if (arg2?.isCurrent == false)
                {
                    arg1.updateCollectionViewUserInteractionEnabled(false);
                }

                arg1.updateCurrentIndexForTap((int)indexPath.Item);
            };
        }

        public override void WillDisplayCell(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
        {
            var currentcell = cell as TabCollectionCell;
            if (currentcell != null && tabView.layouted)
            {
                var fixedIndex = tabView.isInfinity ? indexPath.Item % tabView.pageTabItemsCount : indexPath.Item;
                currentcell.isCurrent = fixedIndex == (tabView.currentIndex % tabView.pageTabItemsCount);
            }
        }
    }
}