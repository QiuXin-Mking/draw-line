# Technical Design

Integrate existing M02 import and CAD geometry into the persistent shell hosts. CAD state owns center canvas drawing and the right CAD property pane; it does not create another page shell. A small state adapter exposes import/line classification/selection modes to the fixed host. Unsupported drawing commands remain TODO while their evidenced controls/tooltips are reproduced.

Tests assert M02 routing, center-host content replacement without nested shell UI, right-field order/defaults, rulers/tooltips, and honest unsupported behavior.
