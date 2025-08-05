// Dane przykładowe - spis treści
const treeData = [
    {
        id: 'intro',
        title: 'Wprowadzenie',
        children: [
            {
                id: 'getting-started',
                title: 'Rozpoczęcie pracy',
                children: [
                    {
                        id: 'installation',
                        title: 'Instalacja',
                        children: [
                            { id: 'install-windows', title: 'Instalacja na Windows' },
                            { id: 'install-linux', title: 'Instalacja na Linux' },
                            { id: 'install-mac', title: 'Instalacja na macOS' }
                        ]
                    },
                    {
                        id: 'configuration',
                        title: 'Konfiguracja',
                        children: [
                            { id: 'config-basic', title: 'Podstawowa konfiguracja' },
                            { id: 'config-advanced', title: 'Zaawansowana konfiguracja' }
                        ]
                    }
                ]
            },
            {
                id: 'overview',
                title: 'Przegląd systemu',
                children: [
                    { id: 'architecture', title: 'Architektura' },
                    { id: 'components', title: 'Komponenty' }
                ]
            }
        ]
    },
    {
        id: 'api',
        title: 'API Reference',
        children: [
            {
                id: 'endpoints',
                title: 'Endpointy',
                children: [
                    { id: 'get-users', title: 'GET /users' },
                    { id: 'post-users', title: 'POST /users' },
                    { id: 'put-users', title: 'PUT /users' }
                ]
            },
            {
                id: 'models',
                title: 'Modele danych',
                children: [
                    { id: 'user-model', title: 'Model User' },
                    { id: 'product-model', title: 'Model Product' }
                ]
            }
        ]
    },
    {
        id: 'tutorials',
        title: 'Tutoriale',
        children: [
            { id: 'tutorial-1', title: 'Pierwszy tutorial' },
            { id: 'tutorial-2', title: 'Drugi tutorial' },
            { id: 'tutorial-3', title: 'Trzeci tutorial' }
        ]
    },
    {
        id: 'faq',
        title: 'FAQ',
        children: [
            { id: 'faq-general', title: 'Ogólne pytania' },
            { id: 'faq-technical', title: 'Pytania techniczne' }
        ]
    }
];

// Wszystkie dokumenty są ładowane dynamicznie z plików HTML

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
                // Rozwiń
                toggle.classList.remove('collapsed');
                toggle.classList.add('expanded');
                if (children) {
                    children.classList.add('expanded');
                }
            } else if (toggle && toggle.classList.contains('expanded')) {
                // Zwiń
                toggle.classList.remove('expanded');
                toggle.classList.add('collapsed');
                if (children) {
                    children.classList.remove('expanded');
                }
            }

            // Aktywuj element
            this.setActiveItem(treeItem);
            
            // Załaduj dokument
            const itemId = treeItem.dataset.id;
            loadDocument(itemId);
        });
    }

    setActiveItem(item) {
        // Usuń poprzednią aktywną pozycję
        if (this.activeItem) {
            this.activeItem.querySelector('.tree-content').classList.remove('active');
        }
        
        // Ustaw nową aktywną pozycję
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

// Inicjalizacja
let treeView;
let isResizing = false;
let startX, startWidth;

document.addEventListener('DOMContentLoaded', () => {
    const treeContainer = document.getElementById('treeContainer');
    treeView = new TreeView(treeContainer, treeData);

    // Przyciski collapse/expand all
    document.getElementById('collapseAll').addEventListener('click', () => {
        treeView.collapseAll();
    });

    document.getElementById('expandAll').addEventListener('click', () => {
        treeView.expandAll();
    });

    // Przycisk chowania panelu
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
    
    // Ładuj dokument z pliku HTML
    const docPath = `documents/${docId}.html`;
    
    fetch(docPath)
        .then(response => {
            if (response.ok) {
                return response.text();
            } else {
                throw new Error('Dokument nie został znaleziony');
            }
        })
        .then(content => {
            mainContent.innerHTML = content;
        })
        .catch(error => {
            console.error('Błąd ładowania dokumentu:', error);
            mainContent.innerHTML = '<div class="document-placeholder">Dokument nie został znaleziony</div>';
        });
} 