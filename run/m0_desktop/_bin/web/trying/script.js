// Tree data will be loaded from tree.json file
let treeData = [];

// All documents are loaded dynamically from HTML files

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
	loadDocument_index();
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
                // Expand
                toggle.classList.remove('collapsed');
                toggle.classList.add('expanded');
                if (children) {
                    children.classList.add('expanded');
                }
            } else if (toggle && toggle.classList.contains('expanded')) {
                // Collapse
                toggle.classList.remove('expanded');
                toggle.classList.add('collapsed');
                if (children) {
                    children.classList.remove('expanded');
                }
            }

            // Activate element
            this.setActiveItem(treeItem);
            
            // Load document
            const itemId = treeItem.dataset.id;
            loadDocument(itemId);
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
    }
}

// Initialization
let treeView;
let isResizing = false;
let startX, startWidth;
let savedSidebarWidth = 250; // Default width

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

    // Collapse/expand all buttons
    document.getElementById('collapseAll').addEventListener('click', () => {
        treeView.collapseAll();
    });

    document.getElementById('expandAll').addEventListener('click', () => {
        treeView.expandAll();
    });

    // Panel hide button
    document.getElementById('toggleBtn').addEventListener('click', (e) => {
        e.stopPropagation();
        e.preventDefault();
        const sidebar = document.getElementById('sidebar');
        const toggleBtn = document.getElementById('toggleBtn');
        const isCollapsed = sidebar.classList.contains('collapsed');
        
        if (isCollapsed) {
            // Expand sidebar - remove collapsed class first
            sidebar.classList.remove('collapsed');
            // Remove all important inline styles first
            sidebar.style.removeProperty('flex-basis');
            sidebar.style.removeProperty('flex-shrink');
            sidebar.style.removeProperty('flex-grow');
            sidebar.style.removeProperty('min-width');
            sidebar.style.removeProperty('max-width');
            // Wait one frame, then set saved width
            requestAnimationFrame(() => {
                sidebar.style.setProperty('flex-basis', savedSidebarWidth + 'px', 'important');
                sidebar.style.setProperty('flex-shrink', '0', 'important');
                sidebar.style.setProperty('flex-grow', '0', 'important');
                sidebar.style.setProperty('min-width', '200px', 'important');
                sidebar.style.setProperty('max-width', '400px', 'important');
            });
            toggleBtn.textContent = '◀';
        } else {
            // Collapse sidebar - save current flex-basis first
            const computedStyle = window.getComputedStyle(sidebar);
            let currentWidth = sidebar.offsetWidth || 250;
            
            // Try to get flex-basis from computed style
            const flexBasis = computedStyle.flexBasis;
            if (flexBasis && flexBasis !== 'auto' && flexBasis !== '0px') {
                const parsed = parseInt(flexBasis);
                if (!isNaN(parsed) && parsed > 0) {
                    currentWidth = parsed;
                }
            }
            
            // Check inline style
            if (sidebar.style.flexBasis) {
                const parsed = parseInt(sidebar.style.flexBasis);
                if (!isNaN(parsed) && parsed > 0) {
                    currentWidth = parsed;
                }
            }
            
            if (currentWidth > 0 && currentWidth < 500) {
                savedSidebarWidth = Math.round(currentWidth);
            }
            
            // Set all size properties to 0 to collapse sidebar completely
            sidebar.style.setProperty('flex-basis', '0px', 'important');
            sidebar.style.setProperty('flex-shrink', '1', 'important');
            sidebar.style.setProperty('flex-grow', '0', 'important');
            sidebar.style.setProperty('width', '0px', 'important');
            sidebar.style.setProperty('min-width', '0px', 'important');
            sidebar.style.setProperty('max-width', '0px', 'important');
            sidebar.style.setProperty('overflow', 'hidden', 'important');
            
            // Add collapsed class (CSS will hide header and tree-container)
            sidebar.classList.add('collapsed');
            toggleBtn.textContent = '▶';
        }
    });

    // Resize handle
    const resizeHandle = document.getElementById('resizeHandle');
    const sidebar = document.getElementById('sidebar');

    resizeHandle.addEventListener('mousedown', (e) => {
        isResizing = true;
        startX = e.clientX;
        startWidth = sidebar.offsetWidth;
        document.body.style.cursor = 'col-resize';
        e.preventDefault();
    });

    document.addEventListener('mousemove', (e) => {
        if (!isResizing) return;
        
        const deltaX = e.clientX - startX;
        const newWidth = Math.max(200, Math.min(400, startWidth + deltaX));
        // Use flex-basis for consistency
        sidebar.style.setProperty('flex-basis', newWidth + 'px', 'important');
        sidebar.style.setProperty('flex-shrink', '0', 'important');
        sidebar.style.setProperty('flex-grow', '0', 'important');
        sidebar.style.setProperty('min-width', '200px', 'important');
        sidebar.style.setProperty('max-width', '400px', 'important');
        // Update saved width if sidebar is not collapsed
        if (!sidebar.classList.contains('collapsed')) {
            savedSidebarWidth = newWidth;
        }
    });

    document.addEventListener('mouseup', () => {
        if (isResizing) {
            isResizing = false;
            document.body.style.cursor = '';
            // Update saved width after resize is complete
            const sidebar = document.getElementById('sidebar');
            if (!sidebar.classList.contains('collapsed')) {
                const currentWidth = sidebar.offsetWidth || parseInt(sidebar.style.width) || 250;
                if (currentWidth > 0 && currentWidth < 500) {
                    savedSidebarWidth = Math.round(currentWidth);
                }
            }
        }
    });
});

function loadDocument_index() {
    const mainContent = document.getElementById('mainContent');
    
    // Load document from HTML file
    const docPath = `index`;
    
    fetch(docPath)
        .then(response => {
            if (response.ok) {
                return response.text();
            } else {
                mainContent.innerHTML = '<div class="document-placeholder">Select an item from the left panel to load the document</div>';
            }
        })
        .then(content => {
            mainContent.innerHTML = content;
        })
        .catch(error => {
            console.error('Error loading document:', error);
            mainContent.innerHTML = '<div class="document-placeholder">Select an item from the left panel to load the document</div>';
        });
} 

function loadDocument(docId) {
    const mainContent = document.getElementById('mainContent');
    
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
        })
        .catch(error => {
            console.error('Error loading document:', error);
            mainContent.innerHTML = '<div class="document-placeholder">Document not found</div>';
        });
} 