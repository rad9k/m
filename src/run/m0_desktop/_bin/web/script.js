// Tree data will be loaded from tree.json file
let treeData = [];

// All documents are loaded dynamically from HTML files

// ===== URL / HASH ROUTING =====
// The custom HTTP server only returns index.html when the request targets the
// root directory, so we cannot encode the document path in the URL pathname
// (any non-root path would be interpreted by the server as a document request
// and would skip loading the SPA shell). Hash fragments are never sent to the
// server, which makes them the portable way to store the currently displayed
// document in the address bar. The encoded value stays human readable: most
// browsers render it with the original spaces/diacritics in the address bar,
// and even when copied out it only gets RFC-3986 percent encoding.
function GetDocIdFromUrl() {
    const hash = window.location.hash;
    if (!hash || hash.length <= 1) {
        return null;
    }
    const raw = hash.substring(1);
    try {
        return decodeURIComponent(raw);
    } catch (error) {
        return raw;
    }
}

function BuildHashForDocId(docId) {
    return '#' + encodeURIComponent(docId);
}

function UpdateUrlForDocId(docId, replaceCurrentEntry) {
    const newHash = BuildHashForDocId(docId);
    if (window.location.hash === newHash) {
        return;
    }
    const state = { docId: docId };
    if (replaceCurrentEntry) {
        history.replaceState(state, '', newHash);
    } else {
        history.pushState(state, '', newHash);
    }
}

function ClearUrlDocId(replaceCurrentEntry) {
    if (!window.location.hash) {
        return;
    }
    const urlWithoutHash = window.location.pathname + window.location.search;
    if (replaceCurrentEntry) {
        history.replaceState({}, '', urlWithoutHash);
    } else {
        history.pushState({}, '', urlWithoutHash);
    }
}

class TreeView {
    constructor(container, data) {
        this.container = container;
        this.data = data;
        this.activeItem = null;
        this.init();
    }

    init() {
        this.render();
        this.bindEvents();

        const initialDocId = GetDocIdFromUrl();
        if (initialDocId) {
            const matched = this.activateItemById(initialDocId);
            loadDocument(initialDocId);
            if (!matched) {
                // Keep the hash in the URL so the user still sees what was
                // requested even if the tree does not contain this id.
                UpdateUrlForDocId(initialDocId, true);
            }
        } else {
            loadDocument_index();
        }
    }

    findItemElementById(id) {
        if (id === null || id === undefined) {
            return null;
        }
        const selector = `.tree-item[data-id="${CSS.escape(String(id))}"]`;
        return this.container.querySelector(selector);
    }

    expandAncestorsOf(itemElement) {
        let currentNode = itemElement.parentElement;
        while (currentNode && currentNode !== this.container) {
            if (currentNode.classList && currentNode.classList.contains('tree-children')) {
                currentNode.classList.add('expanded');
                currentNode.style.maxHeight = 'none';
                const parentItem = currentNode.parentElement;
                if (parentItem && parentItem.classList.contains('tree-item')) {
                    const parentToggle = parentItem.querySelector(':scope > .tree-content > .tree-toggle');
                    if (parentToggle && parentToggle.classList.contains('collapsed')) {
                        parentToggle.classList.remove('collapsed');
                        parentToggle.classList.add('expanded');
                    }
                }
            }
            currentNode = currentNode.parentElement;
        }
    }

    activateItemById(id) {
        const itemElement = this.findItemElementById(id);
        if (!itemElement) {
            return false;
        }
        this.expandAncestorsOf(itemElement);
        this.setActiveItem(itemElement);
        const activeContent = itemElement.querySelector(':scope > .tree-content');
        if (activeContent && typeof activeContent.scrollIntoView === 'function') {
            try {
                activeContent.scrollIntoView({ block: 'nearest' });
            } catch (error) {
                activeContent.scrollIntoView();
            }
        }
        return true;
    }

    render() {
        this.container.innerHTML = '';
        this.data.forEach(item => {
            this.renderItem(item, this.container);
        });
    }

    renderItem(item, parent) {
        const itemDiv = document.createElement('div');
        itemDiv.className = 'tree-item';
        itemDiv.dataset.id = item.id;

        const contentDiv = document.createElement('div');
        contentDiv.className = 'tree-content';

        const toggleDiv = document.createElement('div');
        toggleDiv.className = item.children ? 'tree-toggle collapsed' : 'tree-toggle leaf';

        const titleSpan = document.createElement('span');
        titleSpan.textContent = item.title;

        contentDiv.appendChild(toggleDiv);
        contentDiv.appendChild(titleSpan);
        itemDiv.appendChild(contentDiv);

        if (item.children) {
            const childrenDiv = document.createElement('div');
            childrenDiv.className = 'tree-children';
            item.children.forEach(child => {
                this.renderItem(child, childrenDiv);
            });
            itemDiv.appendChild(childrenDiv);
        }

        parent.appendChild(itemDiv);
    }

    bindEvents() {
        this.container.addEventListener('click', (e) => {
            const treeContent = e.target.closest('.tree-content');
            if (!treeContent) return;

            const treeItem = treeContent.closest('.tree-item');
            const toggle = treeContent.querySelector('.tree-toggle');
            const children = treeItem.querySelector('.tree-children');

            if (toggle && toggle.classList.contains('collapsed')) {
                toggle.classList.remove('collapsed');
                toggle.classList.add('expanded');
                if (children) {
                    children.classList.add('expanded');
                    setTimeout(() => {
                        this.container.querySelectorAll('.tree-children.expanded').forEach(el => {
                            el.style.maxHeight = 'none';
                        });
                    }, 350);
                }
            } else if (toggle && toggle.classList.contains('expanded')) {
                toggle.classList.remove('expanded');
                toggle.classList.add('collapsed');
                if (children) {
                    children.style.maxHeight = children.scrollHeight + 'px';
                    children.offsetHeight;
                    children.style.maxHeight = '';
                    children.classList.remove('expanded');
                }
            }

            // Activate element
            this.setActiveItem(treeItem);
            
            // Load document
            const itemId = treeItem.dataset.id;
            loadDocument(itemId);
            UpdateUrlForDocId(itemId, false);

            if (!treeItem.querySelector('.tree-children') && isMobileViewport()) {
                closeHamburgerMenu();
            }
        });
    }

    setActiveItem(item) {
        // Remove previous active item
        if (this.activeItem) {
            this.activeItem.querySelector('.tree-content').classList.remove('active');
        }
        
        // Set new active item
        this.activeItem = item;
        item.querySelector('.tree-content').classList.add('active');
    }

    collapseAll() {
        this.container.querySelectorAll('.tree-children.expanded').forEach(el => {
            el.style.maxHeight = '';
        });

        const toggles = this.container.querySelectorAll('.tree-toggle.expanded');
        const children = this.container.querySelectorAll('.tree-children.expanded');
        
        toggles.forEach(toggle => {
            toggle.classList.remove('expanded');
            toggle.classList.add('collapsed');
        });
        
        children.forEach(child => {
            child.classList.remove('expanded');
        });
    }

    expandAll() {
        const toggles = this.container.querySelectorAll('.tree-toggle.collapsed');
        const children = this.container.querySelectorAll('.tree-children:not(.expanded)');
        
        toggles.forEach(toggle => {
            toggle.classList.remove('collapsed');
            toggle.classList.add('expanded');
        });
        
        children.forEach(child => {
            child.classList.add('expanded');
        });

        this.container.querySelectorAll('.tree-children.expanded').forEach(el => {
            el.style.maxHeight = 'none';
        });
    }
}

// Initialization
let treeView;
let isResizing = false;
let startX, startWidth;

function isMobileViewport() {
    return window.matchMedia('(max-width: 768px)').matches;
}

function openHamburgerMenu() {
    document.getElementById('sidebar').classList.add('hamburger-open');
    document.getElementById('hamburgerOverlay').classList.add('active');
    document.getElementById('hamburgerBtn').classList.add('active');
}

function closeHamburgerMenu() {
    document.getElementById('sidebar').classList.remove('hamburger-open');
    document.getElementById('hamburgerOverlay').classList.remove('active');
    document.getElementById('hamburgerBtn').classList.remove('active');
}

function getDefaultSidebarWidth() {
    return 305;
}

function applySidebarWidth(sidebar, toggleBtn, width) {
    sidebar.style.width = width + 'px';
    if (!sidebar.classList.contains('collapsed')) {
        toggleBtn.style.left = width + 'px';
    }
}

function getSidebarMinWidth() {
    return 160;
}

function getSidebarMaxWidth() {
    return Math.min(400, Math.max(getSidebarMinWidth(), window.innerWidth - 40));
}

function refreshIOSPdfEmbeds() {
    (function () {
        const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent);

        if (!isIOS) return;

        const frames = document.querySelectorAll('iframe.pdf-embed');
        const mozillaViewerPrefix = 'https://mozilla.github.io/pdf.js/web/viewer.html?file=';

        frames.forEach(frame => {
            const src = frame.getAttribute('src') || '';
            if (src.startsWith(mozillaViewerPrefix)) {
                frame.remove();
            }
        });
    })();
}

// Load tree data from JSON file
async function loadTreeData() {
    try {
        const response = await fetch('tree.json');
        if (response.ok) {
            treeData = await response.json();
            return true;
        } else {
            console.error('Failed to load tree.json');
            return false;
        }
    } catch (error) {
        console.error('Error loading tree.json:', error);
        return false;
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    // Load tree data first
    const dataLoaded = await loadTreeData();
    if (!dataLoaded) {
        console.error('Could not load tree data, using empty tree');
        treeData = [];
    }
    
    const treeContainer = document.getElementById('treeContainer');
    treeView = new TreeView(treeContainer, treeData);

    // React to the user navigating through browser history or editing the hash
    // directly in the address bar. pushState we perform ourselves does not
    // trigger hashchange, so this handler only runs for user-initiated changes.
    window.addEventListener('hashchange', () => {
        const docId = GetDocIdFromUrl();
        if (docId) {
            if (treeView) {
                treeView.activateItemById(docId);
            }
            loadDocument(docId);
        } else {
            if (treeView && treeView.activeItem) {
                treeView.activeItem.querySelector('.tree-content').classList.remove('active');
                treeView.activeItem = null;
            }
            loadDocument_index();
        }
    });

    // Collapse/expand all buttons
    document.getElementById('collapseAll').addEventListener('click', () => {
        treeView.collapseAll();
    });

    document.getElementById('expandAll').addEventListener('click', () => {
        treeView.expandAll();
    });

	let isCollapsed = false;
	let savedWidth = 0;
	
	const sidebar = document.getElementById('sidebar');
	const toggleBtn = document.getElementById('toggleBtn');
	const hamburgerBtn = document.getElementById('hamburgerBtn');
	const hamburgerOverlay = document.getElementById('hamburgerOverlay');

	if (!isMobileViewport()) {
		applySidebarWidth(sidebar, toggleBtn, getDefaultSidebarWidth());
		toggleBtn.classList.add('sidebar-visible');
		toggleBtn.style.left = sidebar.offsetWidth + 'px';
	}

	hamburgerBtn.addEventListener('click', () => {
		if (sidebar.classList.contains('hamburger-open')) {
			closeHamburgerMenu();
		} else {
			openHamburgerMenu();
		}
	});

	hamburgerOverlay.addEventListener('click', () => {
		closeHamburgerMenu();
	});

    toggleBtn.addEventListener('click', () => {
		if(isCollapsed==false){
			isCollapsed = true;
			savedWidth = sidebar.style.width || sidebar.offsetWidth + 'px';
			
			sidebar.classList.add('collapsed');
			toggleBtn.classList.remove('sidebar-visible');
			toggleBtn.textContent = '▶';
			toggleBtn.style.left = '0px';
			
			setTimeout(() => {
				sidebar.style.width = '0px';
				sidebar.style.padding = '0';
				sidebar.style.minWidth = '0';
			}, 300);
		}else{
			isCollapsed = false;
			const widthToRestore = savedWidth || (getDefaultSidebarWidth() + 'px');
			const widthValue = parseInt(widthToRestore) || getDefaultSidebarWidth();
			
			sidebar.style.width = widthToRestore;
			sidebar.style.padding = '';
			sidebar.style.minWidth = '';
			
			sidebar.offsetWidth;
			
			sidebar.classList.remove('collapsed');
			toggleBtn.classList.add('sidebar-visible');
			toggleBtn.style.left = widthValue + 'px';
			toggleBtn.textContent = '◀';
		}
    });

    window.addEventListener('resize', () => {
        if (isMobileViewport()) {
            closeHamburgerMenu();
            return;
        }

        if (isCollapsed || isResizing) {
            return;
        }

        const defaultWidth = getDefaultSidebarWidth();
        applySidebarWidth(sidebar, toggleBtn, defaultWidth);
        savedWidth = defaultWidth + 'px';
    });

    // Resize handle
    const resizeHandle = document.getElementById('resizeHandle');

    resizeHandle.addEventListener('pointerdown', (e) => {
        if (isCollapsed) {
            return;
        }

        isResizing = true;
        startX = e.clientX;
        startWidth = sidebar.offsetWidth;
        sidebar.classList.add('no-transition');
        document.body.style.cursor = 'col-resize';
        resizeHandle.setPointerCapture(e.pointerId);
        e.preventDefault();
    });

    document.addEventListener('pointermove', (e) => {
        if (!isResizing) return;
        
        const deltaX = e.clientX - startX;
        const minWidth = getSidebarMinWidth();
        const maxWidth = getSidebarMaxWidth();
        const newWidth = Math.max(minWidth, Math.min(maxWidth, startWidth + deltaX));
        sidebar.style.width = newWidth + 'px';
        toggleBtn.style.left = newWidth + 'px';
        savedWidth = newWidth + 'px';
        e.preventDefault();
    });

    const stopResizing = () => {
        if (isResizing) {
            isResizing = false;
            sidebar.classList.remove('no-transition');
            document.body.style.cursor = '';
        }
    };

    document.addEventListener('pointerup', stopResizing);
    document.addEventListener('pointercancel', stopResizing);
});

// ===== IMAGE VIEWER (fullscreen for .img-viewable) =====
// Slider range 0..1000 is mapped logarithmically to zoom 10%..1000%,
// so slider value 500 corresponds to 100% zoom (middle position).
const ImageViewer = (function () {
    const SLIDER_MIN = 0;
    const SLIDER_MAX = 1000;
    const ZOOM_MIN_PERCENT = 10;
    const ZOOM_MAX_PERCENT = 150;
    const LOG_MIN = Math.log(ZOOM_MIN_PERCENT);
    const LOG_MAX = Math.log(ZOOM_MAX_PERCENT);

    let overlay, titleEl, slider, zoomValueEl, closeBtn, contentEl, imgEl;
    let baseWidth = 0;
    let baseHeight = 0;
    let currentZoomPercent = 100;
    let initialized = false;

    function sliderToZoomPercent(sliderValue) {
        const ratio = (sliderValue - SLIDER_MIN) / (SLIDER_MAX - SLIDER_MIN);
        const logZoom = LOG_MIN + ratio * (LOG_MAX - LOG_MIN);
        return Math.exp(logZoom);
    }

    function zoomPercentToSlider(zoomPercent) {
        const clamped = Math.max(ZOOM_MIN_PERCENT, Math.min(ZOOM_MAX_PERCENT, zoomPercent));
        const logZoom = Math.log(clamped);
        const ratio = (logZoom - LOG_MIN) / (LOG_MAX - LOG_MIN);
        return SLIDER_MIN + ratio * (SLIDER_MAX - SLIDER_MIN);
    }

    function applyZoom(zoomPercent) {
        currentZoomPercent = zoomPercent;
        const scale = zoomPercent / 100;
        const scaledWidth = baseWidth * scale;
        const scaledHeight = baseHeight * scale;
        imgEl.style.width = scaledWidth + 'px';
        imgEl.style.height = scaledHeight + 'px';
        imgEl.style.transform = 'none';
        zoomValueEl.textContent = Math.round(zoomPercent) + '%';
    }

    function centerScrollIfSmall() {
        const cw = contentEl.clientWidth;
        const ch = contentEl.clientHeight;
        const iw = imgEl.offsetWidth;
        const ih = imgEl.offsetHeight;

        if (iw <= cw) {
            imgEl.style.marginLeft = Math.max(0, (cw - iw) / 2) + 'px';
        } else {
            imgEl.style.marginLeft = '0px';
        }

        if (ih <= ch) {
            imgEl.style.marginTop = Math.max(0, (ch - ih) / 2) + 'px';
        } else {
            imgEl.style.marginTop = '0px';
        }
    }

    function open(sourceImg) {
        if (!initialized) return;

        const src = sourceImg.currentSrc || sourceImg.src;
        const altText = sourceImg.getAttribute('alt') || '';

        titleEl.textContent = altText;
        imgEl.alt = altText;

        imgEl.style.width = '';
        imgEl.style.height = '';
        imgEl.style.marginLeft = '';
        imgEl.style.marginTop = '';

        const onLoaded = () => {
            baseWidth = imgEl.naturalWidth || sourceImg.naturalWidth || imgEl.width;
            baseHeight = imgEl.naturalHeight || sourceImg.naturalHeight || imgEl.height;
            slider.value = zoomPercentToSlider(100);
            applyZoom(100);
            centerScrollIfSmall();
            contentEl.scrollTop = 0;
            contentEl.scrollLeft = 0;
        };

        if (imgEl.src !== src) {
            imgEl.onload = onLoaded;
            imgEl.src = src;
        } else {
            onLoaded();
        }

        overlay.classList.add('active');
        overlay.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';
    }

    function close() {
        overlay.classList.remove('active');
        overlay.setAttribute('aria-hidden', 'true');
        document.body.style.overflow = '';
        imgEl.onload = null;
        imgEl.removeAttribute('src');
    }

    function onSliderInput() {
        const zoomPercent = sliderToZoomPercent(parseFloat(slider.value));

        // Keep the current viewport center fixed while zooming.
        const prevScrollLeft = contentEl.scrollLeft;
        const prevScrollTop = contentEl.scrollTop;
        const viewCenterX = prevScrollLeft + contentEl.clientWidth / 2;
        const viewCenterY = prevScrollTop + contentEl.clientHeight / 2;
        const prevWidth = imgEl.offsetWidth || 1;
        const prevHeight = imgEl.offsetHeight || 1;
        const relX = viewCenterX / prevWidth;
        const relY = viewCenterY / prevHeight;

        applyZoom(zoomPercent);
        centerScrollIfSmall();

        const newWidth = imgEl.offsetWidth;
        const newHeight = imgEl.offsetHeight;
        contentEl.scrollLeft = Math.max(0, relX * newWidth - contentEl.clientWidth / 2);
        contentEl.scrollTop = Math.max(0, relY * newHeight - contentEl.clientHeight / 2);
    }

    function init() {
        overlay = document.getElementById('imageViewerOverlay');
        titleEl = document.getElementById('imageViewerTitle');
        slider = document.getElementById('imageViewerSlider');
        zoomValueEl = document.getElementById('imageViewerZoomValue');
        closeBtn = document.getElementById('imageViewerClose');
        contentEl = document.getElementById('imageViewerContent');
        imgEl = document.getElementById('imageViewerImg');

        if (!overlay) return;
        initialized = true;

        slider.addEventListener('input', onSliderInput);
        closeBtn.addEventListener('click', close);

        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) close();
        });

        document.addEventListener('keydown', (e) => {
            if (!overlay.classList.contains('active')) return;
            if (e.key === 'Escape') {
                close();
            }
        });

        window.addEventListener('resize', () => {
            if (overlay.classList.contains('active')) {
                centerScrollIfSmall();
            }
        });

        // Event delegation for dynamically loaded images with the .img-viewable class.
        document.addEventListener('click', (e) => {
            const target = e.target;
            if (target && target.tagName === 'IMG' && target.classList.contains('img-viewable')) {
                e.preventDefault();
                open(target);
            }
        });
    }

    return { init };
})();

document.addEventListener('DOMContentLoaded', () => {
    ImageViewer.init();
});

function loadDocument_index() {
    const mainContent = document.getElementById('mainContent');
    
    // Load document from HTML file
    const docPath = `00 Start`;
    
    fetch(docPath)
        .then(response => {
            if (response.ok) {
				console.log("Cześć!");
                return response.text();
            } else {
                mainContent.innerHTML = '<div class="document-placeholder">Select an item from the left panel to load the document</div>';
            }
        })
        .then(content => {
            mainContent.innerHTML = content;
            refreshIOSPdfEmbeds();
        })
        .catch(error => {
            console.error('Error loading document:', error);
            mainContent.innerHTML = '<div class="document-placeholder">Select an item from the left panel to load the document</div>';
        });		
} 

function loadDocument(docId) {
    const mainContent = document.getElementById('mainContent');
    
    // Przewiń na górę
    mainContent.scrollTop = 0;
    
    // Load document from HTML file
    const docPath = `${docId}`;
    
    fetch(docPath)
        .then(response => {
            if (response.ok) {
                return response.text();
            } else {
                throw new Error('Document not found');
            }
        })
        .then(content => {
            mainContent.innerHTML = content;
            refreshIOSPdfEmbeds();
            // Opcjonalnie: przewiń ponownie po załadowaniu treści
            mainContent.scrollTop = 0;
        })
        .catch(error => {
            console.error('Error loading document:', error);
            mainContent.innerHTML = '<div class="document-placeholder">Document not found</div>';
        });
}