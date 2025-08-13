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
    document.getElementById('toggleBtn').addEventListener('click', () => {
        const sidebar = document.getElementById('sidebar');
        sidebar.classList.toggle('collapsed');
        const toggleBtn = document.getElementById('toggleBtn');
        toggleBtn.textContent = sidebar.classList.contains('collapsed') ? '▶' : '◀';
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
        sidebar.style.width = newWidth + 'px';
    });

    document.addEventListener('mouseup', () => {
        if (isResizing) {
            isResizing = false;
            document.body.style.cursor = '';
        }
    });
});

function loadDocument(docId) {
    const mainContent = document.getElementById('mainContent');
    
    // Load document from HTML file
    const docPath = `documents/${docId}.html`;
    
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