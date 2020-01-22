using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;

namespace TabPageViewController
{
    public class TabView : UIView
        , IUICollectionViewDataSource
        , IUICollectionViewDelegate
        , IUICollectionViewDelegateFlowLayout
    {
        Action<nint, UIPageViewControllerNavigationDirection> pageItemPressedBlock;
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

        public TabView(bool _isInfinity, TabPageOption _option) : base(CGRect.Empty)
        {
            option = _option;
            isInfinity = _isInfinity;
            //Bundle(for: TabView.self).loadNibNamed("TabView", owner: self, options: nil)
            //AddSubview(contentView);
            //contentView.backgroundColor = option.tabBackgroundColor.withAlphaComponent(option.tabBarAlpha)

        }


        /**
        Center the current cell after page swipe
        */
        private void scrollToHorizontalCenter()
        {
            var indexPath = NSIndexPath.Create(currentIndex, 0);
            collectionView.scrollToItem(at: indexPath, at: .centeredHorizontally, animated: false);
            collectionViewContentOffsetX = collectionView.contentOffset.x;
        }

        /**
        Called in after the transition is complete pages in isInfinityTabPageViewController in the process of updating the current 
        - parameter index: Next Index
        */
        private void updateCurrentIndex(int index, bool shouldScroll)
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
        private void updateCurrentIndexForTap(int index)
        {
            //deselectVisibleCells();

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
                collectionView.scrollToItem(at: indexPath, at: .centeredHorizontally, animated: animated);
                LayoutIfNeeded();
                collectionViewContentOffsetX = 0.0f;
                currentBarViewWidth = 0.0f;
            }

            var cell = collectionView.cellForItem(at: indexPath);

            if (cell is TabCollectionCell)
            {
                currentBarView.isHidden = false;
                if (animated && shouldScroll)
                {
                    cell.isCurrent = true;
                }
                cell.hideCurrentBarView();
                currentBarViewWidthConstraint.constant = cell.frame.width;
                if (!isInfinity)
                {
                    currentBarViewLeftConstraint?.constant = cell.frame.origin.x;
                }
                UIView.Animate(0.2, animation: () =>
                {
                    LayoutIfNeeded();

                },
                completion: () =>
                {
                    if (!animated && shouldScroll)
                    {
                        cell.isCurrent = true;
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
        private void updateCollectionViewUserInteractionEnabled(bool userInteractionEnabled)
        {
            collectionView.isUserInteractionEnabled = userInteractionEnabled;
        }

        /**
     Update all of the cells in the display to the unselected state
     */
        private void deselectVisibleCells()
        {
            collectionView.visibleCells.flatMap { $0 as? TabCollectionCell }.forEach { $0.isCurrent = false }
        }

        #region IUICollectionViewDataSource
        public nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return isInfinity ? pageTabItemsCount * 3 : pageTabItemsCount
        }

        public UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell(withReuseIdentifier: TabCollectionCell.cellIdentifier(), indexPath) as TabCollectionCell;
            configureCell(cell, indexPath: indexPath);
            return cell;
        }

        private void configureCell(TabCollectionCell cell, NSIndexPath indexPath)
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
                    if(indexPath.Item < currentIndex)
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
            if(currentcell != null && layouted)
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
                //currentBarView.isHidden = true;
                var indexPath = NSIndexPath.FromItemSection(currentIndex, 0);
                var cell = collectionView.ItemAt(indexPath) as TabCollectionCell;
                if(!(cell is null))
                {
                    //cell.showCurrentBarView();
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
                collectionView.scrollToItem(at: indexPath, at: .centeredHorizontally, animated: false);
                shouldScrollToItem = false;
            }
        }

        #endregion

        #region IUICollectionViewDelegateFlowLayout

        public CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout collectionViewLayout, NSIndexPath indexPath)
        {
            var size = cachedCellSizes[indexPath];
            if (size != CGSize.Empty) return size;

            configureCell(cellForSize, indexPath);

            var newSize = cellForSize.SizeThatFits(new CGSize(collectionView.Bounds.Width, option.tabHeight));
            cachedCellSizes[indexPath] = newSize;

            return newSize;
        }

        public nfloat GetMinimumInteritemSpacingForSection(UICollectionView collectionView, UICollectionViewLayout collectionViewLayout, int section)
        {
            return 0.0f;
        }
        public nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView, UICollectionViewLayout collectionViewLayout, int section)
        {
            return 0.0f;
        }
        #endregion
    }
}