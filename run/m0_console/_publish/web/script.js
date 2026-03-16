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

	let isCollapsed = false;
	let savedWidth = 0;
	
	// Initialize toggle button position
	const sidebar = document.getElementById('sidebar');
	const toggleBtn = document.getElementById('toggleBtn');
	toggleBtn.classList.add('sidebar-visible');
	toggleBtn.style.left = sidebar.offsetWidth + 'px';
	
    // Panel hide button
    toggleBtn.addEventListener('click', () => {
				
		if(isCollapsed==false){
			isCollapsed = true;
			savedWidth = sidebar.style.width || sidebar.offsetWidth + 'px';
			const currentWidth = sidebar.offsetWidth;
			
			// Start both animations simultaneously
			sidebar.classList.add('collapsed');
			toggleBtn.classList.remove('sidebar-visible');
			toggleBtn.textContent = '▶';
			toggleBtn.style.left = '0px';
			
			// After transform animation, reduce width
			setTimeout(() => {
				sidebar.style.width = '0px';
				sidebar.style.padding = '0';
				sidebar.style.minWidth = '0';
			}, 300);
		}else{
			isCollapsed = false;
			const widthToRestore = savedWidth || '250px';
			const widthValue = parseInt(widthToRestore) || 250;
			
			// Restore width first so sidebar can expand
			sidebar.style.width = widthToRestore;
			sidebar.style.padding = '';
			sidebar.style.minWidth = '';
			
			// Trigger reflow to ensure width is applied before removing collapsed class
			sidebar.offsetWidth;
			
			// Remove collapsed class and animate toggle button simultaneously
			sidebar.classList.remove('collapsed');
			toggleBtn.classList.add('sidebar-visible');
			toggleBtn.style.left = widthValue + 'px';
			toggleBtn.textContent = '◀';
		}
    });

    // Resize handle
    const resizeHandle = document.getElementById('resizeHandle');

    resizeHandle.addEventListener('mousedown', (e) => {
        isResizing = true;
        startX = e.clientX;
        startWidth = sidebar.offsetWidth;
        sidebar.classList.add('no-transition');
        document.body.style.cursor = 'col-resize';
        e.preventDefault();
    });

    document.addEventListener('mousemove', (e) => {
        if (!isResizing) return;
        
        const deltaX = e.clientX - startX;
        const newWidth = Math.max(200, Math.min(400, startWidth + deltaX));
        sidebar.style.width = newWidth + 'px';
        toggleBtn.style.left = newWidth + 'px';
    });

    document.addEventListener('mouseup', () => {
        if (isResizing) {
            isResizing = false;
            sidebar.classList.remove('no-transition');
            document.body.style.cursor = '';
        }
    });
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
            // Opcjonalnie: przewiń ponownie po załadowaniu treści
            mainContent.scrollTop = 0;
        })
        .catch(error => {
            console.error('Error loading document:', error);
            mainContent.innerHTML = '<div class="document-placeholder">Document not found</div>';
        });
}