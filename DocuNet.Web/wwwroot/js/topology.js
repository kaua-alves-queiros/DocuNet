window.topology = {
    instance: null,

    init: function (containerId, elements, dotNetHelper) {
        if (this.instance) {
            this.instance.destroy();
        }

        const containerElement = document.getElementById(containerId);
        if (!containerElement) {
            console.error("Container " + containerId + " not found");
            return;
        }

        this.instance = cytoscape({
            container: containerElement,
            elements: elements,
            style: [
                {
                    selector: 'node',
                    style: {
                        'label': function (ele) {
                            const name = ele.data('label');
                            const ip = ele.data('ip');
                            return ip ? name + '\n' + ip : name;
                        },
                        'text-wrap': 'wrap',
                        'text-max-width': '80px',
                        'background-image': function (ele) {
                            var svg = ele.data('icon');
                            return 'data:image/svg+xml;base64,' + btoa(unescape(encodeURIComponent(svg)));
                        },
                        'background-color': 'data(color)',
                        'background-fit': 'contain',
                        'background-width': '80%',
                        'background-height': '80%',
                        'background-position-x': '50%',
                        'background-position-y': '50%',
                        'background-clip': 'node',
                        'width': 55,
                        'height': 55,
                        'font-size': '12px',
                        'text-valign': 'bottom',
                        'text-halign': 'center',
                        'text-margin-y': 15,
                        'color': '#fff',
                        'font-weight': 'bold',
                        'text-outline-color': '#1a1a1a',
                        'text-outline-width': 2,
                        'border-width': 2,
                        'border-color': '#fff',
                        'border-opacity': 0.5,
                        'shape': 'ellipse'
                    }
                },
                {
                    selector: 'node:selected',
                    style: {
                        'border-width': 4,
                        'border-color': '#7E6FFF',
                        'border-opacity': 1,
                        'box-shadow': '0 0 15px #7E6FFF'
                    }
                },
                {
                    selector: 'edge',
                    style: {
                        'width': 3,
                        'line-color': 'data(color)',
                        'target-arrow-color': 'data(color)',
                        'target-arrow-shape': 'triangle',
                        'curve-style': 'bezier',
                        'text-wrap': 'wrap',
                        'label': function (ele) {
                            return (ele.data('label') ? '🔌 ' + ele.data('label') : '');
                        },
                        'source-label': 'data(sourcePort)',
                        'target-label': 'data(targetPort)',
                        'font-size': '9px',
                        'font-weight': 'bold',
                        'text-rotation': 'none',
                        'source-text-offset': 60,
                        'target-text-offset': 60,
                        'color': '#fff',
                        'text-background-opacity': 0.85,
                        'text-background-color': '#1a1a1a',
                        'text-background-padding': '3px',
                        'text-background-shape': 'roundrectangle',
                        'source-text-background-opacity': 0.9,
                        'source-text-background-color': '#2a2a2a',
                        'target-text-background-opacity': 0.9,
                        'target-text-background-color': '#2a2a2a'
                    }
                },
                {
                    selector: 'edge:selected',
                    style: {
                        'width': 6,
                        'line-color': '#7E6FFF',
                        'target-arrow-color': '#7E6FFF',
                        'box-shadow': '0 0 10px #7E6FFF'
                    }
                }
            ],
            layout: {
                name: 'breadthfirst',
                directed: true,
                padding: 100,
                spacingFactor: 2.6,
                animate: true,
                maximal: false,
                grid: false,
                nodeDimensionsIncludeLabels: true,
                sort: function (a, b) {
                    return a.data('sortWeight') - b.data('sortWeight');
                }
            },
            wheelSensitivity: 0.15,
            pixelRatio: 'auto',
            renderingHint: 'quality'
        });

        const cy = this.instance;

        // Eventos de seleção
        cy.on('tap', 'node', function (evt) {
            const node = evt.target;
            dotNetHelper.invokeMethodAsync('OnElementSelected', 'node', {
                id: node.id(),
                label: node.data('label'),
                ip: node.data('ip')
            });
        });

        cy.on('tap', 'edge', function (evt) {
            const edge = evt.target;
            dotNetHelper.invokeMethodAsync('OnElementSelected', 'edge', {
                id: edge.id(),
                source: edge.data('source'),
                target: edge.data('target'),
                label: edge.data('label'),
                sourcePort: edge.data('sourcePort'),
                targetPort: edge.data('targetPort')
            });
        });

        cy.on('tap', function (evt) {
            if (evt.target === cy) {
                dotNetHelper.invokeMethodAsync('OnElementSelected', null, null);
            }
        });

        this.instance.userZoomingEnabled(true);
        this.instance.userPanningEnabled(true);
        this.instance.boxSelectionEnabled(false);
    },

    focusElement: function (id) {
        if (!this.instance) return;
        const ele = this.instance.getElementById(id);
        if (ele && ele.length > 0) {
            this.instance.elements().unselect();
            ele.select();
            this.instance.animate({
                center: { eles: ele },
                zoom: 1.5,
                duration: 500
            });
        }
    },

    destroy: function () {
        if (this.instance) {
            this.instance.destroy();
            this.instance = null;
        }
    }
};
