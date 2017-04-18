var PackageManagerEntryPoint =
/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// identity function for calling harmony imports with the correct context
/******/ 	__webpack_require__.i = function(value) { return value; };
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, {
/******/ 				configurable: false,
/******/ 				enumerable: true,
/******/ 				get: getter
/******/ 			});
/******/ 		}
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "";
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = 17);
/******/ })
/************************************************************************/
/******/ ([
/* 0 */
/***/ (function(module, exports) {

module.exports = React;

/***/ }),
/* 1 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
/**
 * The Event class stores the callback function together with a name
 * that identifies it.
 */
var Event = (function () {
    function Event(name) {
        this.name = name;
        this.callbacks = [];
    }
    Event.prototype.registerCallback = function (callback) {
        this.callbacks.push(callback);
    };
    Event.prototype.executeCallback = function (params) {
        this.callbacks.forEach(function (callback) {
            try {
                if (callback.length == 0)
                    callback();
                else
                    callback(params);
            }
            catch (e) {
                console.log(e);
            }
        });
    };
    return Event;
}());
exports.Event = Event;
/**
 * The Reactor class stores an array of events registered to it.
 */
var Reactor = (function () {
    function Reactor() {
        this.events = [];
    }
    Reactor.prototype.getEvent = function (eventName) {
        for (var i = 0; i < this.events.length; i++) {
            if (this.events[i].name == eventName) {
                return this.events[i];
            }
        }
    };
    Reactor.prototype.registerEvent = function (eventName, callback) {
        var event = this.getEvent(eventName);
        if (!event) {
            event = new Event(eventName);
            this.events.push(event);
        }
        event.registerCallback(callback);
    };
    Reactor.prototype.raiseEvent = function (name, params) {
        var event = this.getEvent(name);
        if (event != null) {
            event.executeCallback(params);
        }
    };
    return Reactor;
}());
exports.Reactor = Reactor;


/***/ }),
/* 2 */
/***/ (function(module, exports) {

module.exports = ReactDOM;

/***/ }),
/* 3 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var DownloadUtils = __webpack_require__(18);
var InstallButtons = (function (_super) {
    __extends(InstallButtons, _super);
    function InstallButtons(props) {
        var _this = _super.call(this, props) || this;
        _this.state = { installed: false, hasUpdate: true };
        _this.installPackage = _this.installPackage.bind(_this);
        _this.updatePackage = _this.updatePackage.bind(_this);
        _this.uninstallPackage = _this.uninstallPackage.bind(_this);
        return _this;
    }
    InstallButtons.prototype.installPackage = function () {
        DownloadUtils.downloadFile("");
        var state = this.state;
        this.setState({ installed: true, hasUpdate: state.hasUpdate });
    };
    InstallButtons.prototype.updatePackage = function () {
    };
    InstallButtons.prototype.uninstallPackage = function () {
    };
    InstallButtons.prototype.render = function () {
        if (!this.state.installed) {
            return (React.createElement("div", null,
                React.createElement("div", { className: "InstallButtons", onClick: this.installPackage }, "INSTALL")));
        }
        else {
            return (React.createElement("div", { className: "InstallButtonGroup" },
                React.createElement("div", { className: "InstallButtons", onClick: this.uninstallPackage }, "UNINSTALL"),
                React.createElement("div", { className: "InstallButtons", onClick: this.updatePackage, hidden: !this.state.hasUpdate }, "UPDATE")));
        }
    };
    return InstallButtons;
}(React.Component));
exports.InstallButtons = InstallButtons;


/***/ }),
/* 4 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var StarRating = (function (_super) {
    __extends(StarRating, _super);
    function StarRating(props) {
        var _this = _super.call(this, props) || this;
        _this.state = { hoveredRate: -1, selectedRate: -1 };
        return _this;
    }
    StarRating.prototype.onStarMouseOver = function (event) {
        this.setState({ hoveredRate: event.target.value });
    };
    StarRating.prototype.onStarClick = function (event) {
        // If the user has not yet given any ratings yet
        if (this.state.selectedRate == -1) {
            this.setState({ selectedRate: event.target.value });
            this.setState({ hoveredRate: -1 });
        }
    };
    StarRating.prototype.onStarMouseLeave = function (event) {
        this.setState({ hoveredRate: -1 });
    };
    StarRating.prototype.render = function () {
        // First, the given value to rate
        var rate = this.props.rate;
        var starColorClass = "StarRatingGrey";
        var emptyStarColorClass = "StarRatingGrey";
        if (this.state.selectedRate != -1) {
            // Check if there is a new rating given by the user. If there is, assign it to rate
            rate = this.state.selectedRate;
            starColorClass = "StarRatingWhite";
        }
        else if (this.state.hoveredRate != -1) {
            // If the user has not yet given any ratings, allow the user to hover over the stars
            // to give it a rating. Check if the stars are being hovered and update the UI to show this
            rate = this.state.hoveredRate;
            starColorClass = "StarRatingYellow";
        }
        var stars = [];
        var solidStar = Math.floor(rate);
        var halfStart = 0;
        if (rate - solidStar > 0.5) {
            halfStart = 1;
        }
        var emptyStar = 5 - halfStart - solidStar;
        var index = 1;
        for (var i = 0; i < solidStar; i++) {
            var starClassName = starColorClass + " fa fa-star";
            stars.push(React.createElement("button", { className: starClassName, value: index, onMouseOver: this.onStarMouseOver.bind(this), onClick: this.onStarClick.bind(this), onMouseLeave: this.onStarMouseLeave.bind(this) }));
            index++;
        }
        if (halfStart == 1) {
            var starClassName = starColorClass + " fa fa-star-half-o";
            stars.push(React.createElement("button", { className: starClassName, value: index, onMouseOver: this.onStarMouseOver.bind(this), onClick: this.onStarClick.bind(this), onMouseLeave: this.onStarMouseLeave.bind(this) }));
            index++;
        }
        for (var i = 0; i < emptyStar; i++) {
            var starClassName = emptyStarColorClass + " fa fa-star-o";
            stars.push(React.createElement("button", { className: starClassName, value: index, onMouseOver: this.onStarMouseOver.bind(this), onClick: this.onStarClick.bind(this), onMouseLeave: this.onStarMouseLeave.bind(this) }));
            index++;
        }
        // Display stars on the page
        return (React.createElement("div", { className: "StarRating" }, stars));
    };
    return StarRating;
}(React.Component));
exports.StarRating = StarRating;


/***/ }),
/* 5 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
function generatePackageIcon(pkg, dimensions) {
    var canvas = document.createElement('canvas');
    var context = canvas.getContext('2d');
    context.canvas.width = dimensions;
    context.canvas.height = dimensions;
    context.rect(0, 0, dimensions, dimensions);
    context.fillStyle = '#eee';
    context.fill();
    context.font = "bold " + 0.85 * dimensions + "px \"Open Sans\"";
    context.textAlign = 'center';
    context.textBaseline = 'middle';
    context.fillStyle = '#' + pkg._id.substr(0, 6); // the first 6 hex digits of package guid
    context.fillText(pkg.name.charAt(0).toUpperCase(), dimensions / 2, dimensions / 2.05);
    return canvas.toDataURL('image/png');
}
exports.generatePackageIcon = generatePackageIcon;


/***/ }),
/* 6 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var EventHandler_1 = __webpack_require__(1);
/**
 * This is the singleton controller class that manages all other sub-systems of
 * the Package Manager UI module. It is important to know that this class makes
 * no assumption of other React components. Its job is to interact with  external
 * package manager systems (e.g. Dynamo C# logic), and raise events that various
 * React components can register to.
 */
var PackageController = (function () {
    /**
     * Constructs an instance of PackageController object
     * @constructor
     */
    function PackageController() {
        this.reactor = null;
        this.requestHandler = {};
        this.reactor = new EventHandler_1.Reactor();
        this.on = this.on.bind(this);
        this.raiseEvent = this.raiseEvent.bind(this);
        this.registerRequestHandler = this.registerRequestHandler.bind(this);
        this.request = this.request.bind(this);
    }
    /**
     * This method allows any external component to subscribe to events that
     * PackageController is raising. Currently available events include:
     *
     *  onPackageDownloadStarted(packageId: string)
     *  onDownloadProgressUpdated(packageId: string, percentage: number)
     *  onPackageDownloadEnded(packageId: string)
     *
     * @param {string} eventName - The name of the event as outlined above
     * @param {Function} callback - The callback function to be called when
     * the event is raised. The actual arguments for this callback function
     * vary depends on the specific event type
     */
    PackageController.prototype.on = function (eventName, callback) {
        this.reactor.registerEvent(eventName, callback);
    };
    /**
     * This method should not be called from outside of PackageController by
     * design. If there is a component that requires to call this directly,
     * then perhaps there is something that's quite wrong. Consider removing
     * this method in near future.
     *
     * @param {string} name - Name of the event to raise
     * @param {any} params - Optional parameters associated with the event.
     * The value of this parameter depends on the specific event type.
     */
    PackageController.prototype.raiseEvent = function (name, params) {
        this.reactor.raiseEvent(name, params);
    };
    /**
     * This method registers the callback function when the value of a given
     * named variable is requested. See PackageController.request method for
     * more information on each available variable name.
     *
     * Note that if called more than once, this method replaces the original
     * callback function with the new value.
     *
     * @param {string} variableName The name of the variable.
     * @param {Function} callback The callback function to be invoked when a
     * variable of the given name is queried.
     */
    PackageController.prototype.registerRequestHandler = function (variableName, callback) {
        // Storing the callback function in the property map.
        this.requestHandler[variableName] = callback;
    };
    /**
     * External code calls this method to obtain the value of the given named
     * variable. The execution of this method may or may not be asynchronous
     * depending on the corresponding handler registered in onRequest. Once
     * the value of the variable is obtained, the callback function is invoked
     * with the value.
     *
     * Possible variable and their corresponding data types are as followed:
     *
     *      packageState: string
     *
     * @param {string} variableName The name of the variable whose value is to
     * be retrieved.
     * @param {Function} callback The callback function to be invoked when the
     * variable value is obtained.
     * @param {any[]} argsArray Zero or more argument values to be passed to the
     * subscribed handler for this named variable.
     */
    PackageController.prototype.request = function (variableName, callback) {
        var argsArray = [];
        for (var _i = 2; _i < arguments.length; _i++) {
            argsArray[_i - 2] = arguments[_i];
        }
        // Invokve handler if there's one registered.
        var requestHandler = this.requestHandler[variableName];
        if (requestHandler && callback) {
            var value = requestHandler(argsArray);
            callback(value);
        }
    };
    return PackageController;
}());
exports.PackageController = PackageController;


/***/ }),
/* 7 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var PackageHeader_1 = __webpack_require__(11);
var PackageDescription_1 = __webpack_require__(10);
var VersionContainer_1 = __webpack_require__(15);
var CloseButton_1 = __webpack_require__(9);
var PackageDetailView = (function (_super) {
    __extends(PackageDetailView, _super);
    function PackageDetailView(props) {
        var _this = _super.call(this, props) || this;
        _this.activePackageJson = null;
        _this.state = {
            activePackageJson: null, activePackageId: _this.props.packageId, packageJsonDownloaded: false
        };
        _this.beginDownloadPackage = _this.beginDownloadPackage.bind(_this);
        _this.beginDownloadPackage(_this.props.packageId); // Begin download
        return _this;
    }
    PackageDetailView.prototype.beginDownloadPackage = function (packageId) {
        var thisObject = this;
        fetch("/package/" + packageId)
            .then(function (response) {
            return response.text();
        }).then(function (jsonString) {
            var pkg = JSON.parse(jsonString);
            thisObject.setState({ packageJsonDownloaded: true, activePackageJson: pkg });
        });
    };
    PackageDetailView.prototype.render = function () {
        var _this = this;
        if (this.props.packageId != this.state.activePackageId) {
            this.setState({ activePackageId: this.props.packageId });
            this.beginDownloadPackage(this.props.packageId);
        }
        if (!this.state.packageJsonDownloaded) {
            return (React.createElement("div", null, "Select package to download..."));
        }
        var dependencies = ["jaz", "juice"];
        var content = this.state.activePackageJson.content;
        return (React.createElement("div", { id: "PackageDetailView", className: "PackageDetailView" },
            this.props.showCloseButton == true ?
                React.createElement(CloseButton_1.CloseButton, { onClickCallback: function () { return _this.props.pkgController.raiseEvent("closeButtonClicked", _this.props.packageId); } }) : null,
            React.createElement(PackageHeader_1.PackageHeader, { pkg: content }),
            React.createElement(PackageDescription_1.PackageDescription, { pkg: content }),
            React.createElement(VersionContainer_1.VersionContainer, { pkgController: this.props.pkgController, pkg: content }))); // Condiitonal rendering of close button based on if the value of showCloseButton is true
    };
    return PackageDetailView;
}(React.Component));
exports.PackageDetailView = PackageDetailView;


/***/ }),
/* 8 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var SearchBar_1 = __webpack_require__(14);
var PackageList_1 = __webpack_require__(13);
var TabComponent = (function (_super) {
    __extends(TabComponent, _super);
    function TabComponent(props) {
        var _this = _super.call(this, props) || this;
        _this.tabCaptions = [];
        _this.tabContents = [];
        _this.filterConfig = {
            searchText: "",
            sortKey: "PackageName",
            sortOrder: "Ascending"
        };
        _this.setActivePackageId = _this.setActivePackageId.bind(_this);
        _this.insertTab = _this.insertTab.bind(_this);
        _this.onSearchChanged = _this.onSearchChanged.bind(_this);
        _this.state = {
            filterChanged: false
        };
        return _this;
    }
    TabComponent.prototype.insertTab = function (caption, content) {
        this.tabCaptions.push(caption);
        this.tabContents.push(content);
    };
    TabComponent.prototype.setActivePackageId = function (id) {
        // Raise the event registered in index
        this.props.tabControl.raiseEvent("packageItemClicked", id);
    };
    TabComponent.prototype.onSearchChanged = function (filterConfig) {
        this.filterConfig = filterConfig;
        this.setState({ filterChanged: true });
    };
    TabComponent.prototype.render = function () {
        var filterConfig = this.filterConfig;
        return (React.createElement("div", { id: "TabContentArea", className: "TabContentArea" },
            React.createElement(SearchBar_1.SearchBar, { onSearchChanged: this.onSearchChanged }),
            React.createElement(PackageList_1.PackageList, { pkgController: this.props.pkgController, filterConfig: filterConfig, setActivePackageId: this.setActivePackageId })));
    };
    return TabComponent;
}(React.Component));
exports.TabComponent = TabComponent;


/***/ }),
/* 9 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var CloseButton = (function (_super) {
    __extends(CloseButton, _super);
    function CloseButton() {
        var _this = _super.call(this) || this;
        _this.state = { visible: false };
        return _this;
    }
    CloseButton.prototype.Display = function (display) {
        this.setState({
            visible: display
        });
    };
    CloseButton.prototype.render = function () {
        var font = "fa fa-times";
        var widget = [];
        widget.push(React.createElement("button", { className: font, style: { background: "transparent", border: "transparent" } }));
        return (React.createElement("div", { className: "CloseDetailView", onClick: this.props.onClickCallback }, widget));
    };
    return CloseButton;
}(React.Component));
exports.CloseButton = CloseButton;


/***/ }),
/* 10 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var PackageDescription = (function (_super) {
    __extends(PackageDescription, _super);
    function PackageDescription(props) {
        var _this = _super.call(this, props) || this;
        _this.state = { expanded: false };
        return _this;
    }
    PackageDescription.prototype.onToggleClick = function () {
        this.setState({ expanded: !this.state.expanded });
    };
    PackageDescription.prototype.render = function () {
        var description = this.props.pkg.description;
        var toggle = null;
        if (description.length > 400) {
            var toggleText = "[Less]";
            if (!this.state.expanded) {
                description = description.substr(0, 400) + "...";
                toggleText = "[More]";
            }
            toggle = React.createElement("span", { className: "PackageDescriptionToggle", onClick: this.onToggleClick.bind(this) }, toggleText);
        }
        var keys = this.props.pkg.keywords.map(function (key) { return React.createElement("span", { className: "Keyword" }, key); });
        return (React.createElement("div", { className: "PackageDescription" },
            React.createElement("div", { className: "DetailSectionHeader" }, "Description"),
            description,
            toggle,
            React.createElement("div", { className: "DetailSection Keywords" }, keys)));
    };
    return PackageDescription;
}(React.Component));
exports.PackageDescription = PackageDescription;


/***/ }),
/* 11 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var JavaScriptUtils = __webpack_require__(5);
var StarRating_1 = __webpack_require__(4);
var PackageHeader = (function (_super) {
    __extends(PackageHeader, _super);
    function PackageHeader(props) {
        return _super.call(this, props) || this;
    }
    PackageHeader.prototype.render = function () {
        var pkg = this.props.pkg;
        var rate = 4.6;
        var iconSource = pkg.icon_url;
        if (!iconSource) {
            iconSource = JavaScriptUtils.generatePackageIcon(pkg, 60);
        }
        var site = React.createElement("a", { href: pkg.site_url, target: "webpage" }, "Website");
        if (pkg.site_url === "")
            site = undefined;
        var repo = React.createElement("a", { href: pkg.repository_url, target: "webpage" }, "Repository");
        if (pkg.repository_url === "")
            repo = undefined;
        var separator = (pkg.site_url === "" || pkg.repository_url === "") ? "" : " | ";
        var starRating = React.createElement(StarRating_1.StarRating, { rate: rate });
        return (React.createElement("div", { className: "PackageHeader" },
            React.createElement("div", { className: "PackageHeaderLeftPanel" },
                React.createElement("div", { className: "PackageIcon" },
                    React.createElement("img", { className: "PackageIcon", src: iconSource }))),
            React.createElement("div", { className: "PackageHeaderRightPanel" },
                React.createElement("div", { className: "PackageName" }, pkg.name),
                React.createElement("div", { className: "PackageAuthor" },
                    "by ",
                    React.createElement("a", { href: "#" }, pkg.maintainers[0].username)),
                React.createElement("div", { className: "PackageCrumbles" },
                    React.createElement("div", { className: "AlignLeft" },
                        starRating,
                        "\u00A0(",
                        pkg.votes,
                        "\u00A0",
                        React.createElement("i", { className: "fa fa-users" }),
                        ")"),
                    React.createElement("div", { className: "AlignLeft" },
                        React.createElement("i", { className: "fa fa-download" }),
                        "\u00A0",
                        pkg.downloads,
                        "\u00A0"),
                    React.createElement("div", { className: "AlignRight" },
                        site,
                        separator,
                        repo)))));
    };
    return PackageHeader;
}(React.Component));
exports.PackageHeader = PackageHeader;


/***/ }),
/* 12 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var JavaScriptUtils = __webpack_require__(5);
var InstallButtons_1 = __webpack_require__(3);
var StarRating_1 = __webpack_require__(4);
var PackageItem = (function (_super) {
    __extends(PackageItem, _super);
    function PackageItem(props) {
        var _this = _super.call(this, props) || this;
        _this.state = { expanded: false };
        _this.toggleExpandState = _this.toggleExpandState.bind(_this);
        return _this;
    }
    PackageItem.prototype.toggleExpandState = function () {
        this.setState({ expanded: !this.state.expanded }); // Toggle boolean value
    };
    PackageItem.prototype.onPackageItemClicked = function () {
        this.props.setSelection(this.props.index, this.props.data._id);
    };
    PackageItem.prototype.render = function () {
        var pkg = this.props.data;
        var description = pkg.description;
        var selectedStyle = "PackageItem ";
        selectedStyle += this.props.selected ? "PackageItemSelected" : "PackageItemBg";
        var iconSource = pkg.icon_url;
        if (!iconSource) {
            iconSource = JavaScriptUtils.generatePackageIcon(pkg, 40);
        }
        return (React.createElement("div", { className: selectedStyle, onClick: this.onPackageItemClicked.bind(this) },
            React.createElement("div", { className: "ItemLeftPanel" },
                React.createElement("img", { className: "PackageIcon", src: iconSource })),
            React.createElement("div", { className: "ItemRightPanel" },
                React.createElement("div", { className: "PackageCaption" },
                    React.createElement("span", { className: "ItemPackageName" }, pkg.name),
                    React.createElement("span", { className: "ItemPackageVersion" }, pkg.versions[0].version)),
                React.createElement("div", { className: "ItemPackageAuthor" },
                    "by ",
                    pkg.maintainers[0].username),
                React.createElement("div", { className: "ItemCrumbles" },
                    React.createElement("div", { className: "ItemStars" },
                        React.createElement(StarRating_1.StarRating, { rate: 4 })),
                    React.createElement(InstallButtons_1.InstallButtons, { pkgController: this.props.pkgController, packageLink: undefined, packageName: pkg.name, packageVersion: pkg.versions[0].version })))));
    };
    return PackageItem;
}(React.Component));
exports.PackageItem = PackageItem;


/***/ }),
/* 13 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var PackageItem_1 = __webpack_require__(12);
var PackageList = (function (_super) {
    __extends(PackageList, _super);
    function PackageList(props) {
        var _this = _super.call(this, props) || this;
        _this.activePackageJson = null;
        _this.state = { selectedIndex: -1, packageJsonDownloaded: false, selectedId: null };
        _this.setSelection = _this.setSelection.bind(_this);
        _this.beginDownloadPackage = _this.beginDownloadPackage.bind(_this);
        _this.beginDownloadPackage();
        return _this;
    }
    PackageList.prototype.beginDownloadPackage = function () {
        var thisObject = this;
        fetch("/packages")
            .then(function (response) {
            return response.text();
        }).then(function (jsonString) {
            thisObject.activePackageJson = JSON.parse(jsonString);
            thisObject.setState({ packageJsonDownloaded: true });
        });
    };
    PackageList.prototype.setSelection = function (index, id) {
        this.setState({ selectedIndex: index, selectedId: id });
        this.props.setActivePackageId(id);
    };
    PackageList.prototype.render = function () {
        var _this = this;
        if (!this.state.packageJsonDownloaded) {
            return (React.createElement("div", null, "Downloading..."));
        }
        var index = 0;
        var filteredPackages = this.activePackageJson.content;
        if (this.props.filterConfig.searchText.length > 0) {
            filteredPackages = this.activePackageJson.content.filter(function (pkg) {
                return pkg.name.toLowerCase().indexOf(_this.props.filterConfig.searchText) >= 0;
            });
        }
        filteredPackages.sort(function (pkg1, pkg2) {
            if (this.props.filterConfig.sortKey == "DownloadCount") {
                return pkg1.downloads - pkg2.downloads;
            }
            else if (this.props.filterConfig.sortKey == "Author") {
                return pkg1.maintainers[0].username.localeCompare(pkg2.maintainers[0].username);
            }
            else if (this.props.filterConfig.sortKey == "RecentlyUpdated") {
                return Date.parse(pkg1.latest_version_update) - Date.parse(pkg2.latest_version_update);
            }
            else if (this.props.filterConfig.sortKey == "Rating") {
                return pkg1.votes - pkg2.votes;
            }
            else {
                return pkg1.name.localeCompare(pkg2.name);
            }
        }.bind(this));
        if (this.props.filterConfig.sortOrder == "Descending") {
            filteredPackages.reverse();
        }
        var packageElements = filteredPackages.map(function (pkg) {
            return React.createElement(PackageItem_1.PackageItem, { pkgController: _this.props.pkgController, index: ++index, data: pkg, selected: index == _this.state.selectedIndex, setSelection: _this.setSelection });
        });
        return (React.createElement("div", { className: "PackageList" }, packageElements));
    };
    return PackageList;
}(React.Component));
exports.PackageList = PackageList;


/***/ }),
/* 14 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var ReactDOM = __webpack_require__(2);
var SearchBar = (function (_super) {
    __extends(SearchBar, _super);
    function SearchBar(props) {
        var _this = _super.call(this, props) || this;
        _this.sortKeys = ["PackageName", "DownloadCount", "Rating", "RecentlyUpdated", "Author"];
        _this.sortOrders = ["Ascending", "Descending"];
        _this.filterConfig = {
            searchText: "",
            sortKey: "PackageName",
            sortOrder: "Ascending"
        };
        _this.state = {
            expanded: false,
            sortKey: _this.filterConfig.sortKey,
            sortOrder: _this.filterConfig.sortOrder
        };
        return _this;
    }
    SearchBar.prototype.componentWillMount = function () {
        window.addEventListener("keydown", this.handleKeyDown.bind(this));
        window.addEventListener("click", this.handleGlobalClick.bind(this));
    };
    SearchBar.prototype.componentWillUnmount = function () {
        window.removeEventListener("keydown", this.handleKeyDown.bind(this));
        window.removeEventListener("click", this.handleGlobalClick.bind(this));
    };
    SearchBar.prototype.handleKeyDown = function (event) {
        switch (event.code) {
            case "Escape":
                this.clearSearch();
                break;
            default:
                break;
        }
    };
    // Collapse list dropdown menu when click happens outside this compnent
    SearchBar.prototype.handleGlobalClick = function (event) {
        if (!ReactDOM.findDOMNode(this).contains(event.target)) {
            this.collapseMenu();
        }
    };
    SearchBar.prototype.collapseMenu = function () {
        this.setState({ expanded: false });
    };
    SearchBar.prototype.clearSearch = function () {
        var searchInput = document.getElementById("SearchInput");
        searchInput.value = '';
        this.filterConfig.searchText = searchInput.value;
        this.props.onSearchChanged(this.filterConfig);
    };
    SearchBar.prototype.onTextChange = function (event) {
        var text = event.target.value.trim().toLowerCase();
        this.filterConfig.searchText = text;
        this.props.onSearchChanged(this.filterConfig);
    };
    SearchBar.prototype.onSortButtonClick = function () {
        this.setState({
            expanded: !this.state.expanded
        });
    };
    SearchBar.prototype.onSortKeyClick = function (event) {
        this.filterConfig.sortKey = event.target.id;
        this.setState({
            sortKey: this.filterConfig.sortKey
        });
        this.props.onSearchChanged(this.filterConfig);
        this.setState({
            expanded: false
        });
    };
    SearchBar.prototype.onSortOrderClick = function (event) {
        this.filterConfig.sortOrder = event.target.id;
        this.setState({
            sortOrder: this.filterConfig.sortOrder
        });
        this.props.onSearchChanged(this.filterConfig);
        this.setState({
            expanded: false
        });
    };
    // Split sortkey on uppercase characters
    SearchBar.prototype.getNameFromSortKey = function (sortKey) {
        return sortKey.match(/[A-Z][a-z]+|[0-9]+/g).join(" ");
    };
    SearchBar.prototype.render = function () {
        var _this = this;
        var options = null;
        var cancelButton = null;
        var optionText = "OptionText";
        var optionTextHighlight = "OptionTextHighlight";
        if (this.filterConfig.searchText.length > 0) {
            cancelButton = React.createElement("div", { className: "CancelButton", onClick: this.clearSearch.bind(this) },
                React.createElement("i", { className: "fa fa-times", "aria-hidden": "true" }));
        }
        if (this.state.expanded) {
            var sortKeys = this.sortKeys.map(function (sortKey) {
                return React.createElement("li", { id: sortKey, className: _this.state.sortKey == sortKey ? optionTextHighlight : optionText, onClick: _this.onSortKeyClick.bind(_this) }, _this.getNameFromSortKey(sortKey));
            });
            var sortOrders = this.sortOrders.map(function (sortOrder) {
                return React.createElement("li", { id: sortOrder, className: _this.state.sortOrder == sortOrder ? optionTextHighlight : optionText, onClick: _this.onSortOrderClick.bind(_this) }, sortOrder);
            });
            options = (React.createElement("div", { className: "Options" },
                React.createElement("div", { className: "SortBy" }, "Sort by: "),
                React.createElement("ul", { className: "SortKeys" }, sortKeys),
                React.createElement("div", { className: "OrderBy" }, "Order: "),
                React.createElement("ul", { className: "SortOrders" }, sortOrders)));
        }
        return (React.createElement("div", { className: "SearchBar" },
            React.createElement("input", { className: "SearchInput", id: "SearchInput", type: "search", placeholder: "Search...", onChange: this.onTextChange.bind(this), onFocus: this.collapseMenu.bind(this) }),
            cancelButton,
            React.createElement("div", { className: "SearchOptionsContainer" },
                React.createElement("div", { className: this.state.expanded ? "SortIconHighlight" : "SortIcon", onClick: this.onSortButtonClick.bind(this) },
                    React.createElement("i", { className: "fa fa-sort-amount-desc", "aria-hidden": "true" })),
                options)));
    };
    return SearchBar;
}(React.Component));
exports.SearchBar = SearchBar;


/***/ }),
/* 15 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var InstallButtons_1 = __webpack_require__(3);
var VersionDetailView_1 = __webpack_require__(16);
function onPackageItemClick() {
    this.setState({ selectedVerIndex: 0 });
}
exports.onPackageItemClick = onPackageItemClick;
var VersionContainer = (function (_super) {
    __extends(VersionContainer, _super);
    function VersionContainer(props) {
        var _this = 
        // Pass in a package JSON object for initialization: 
        // <VersionContainer pkg={listOfPackages[selectedIndex]}/>        
        _super.call(this, props) || this;
        _this.state = { selectedVerIndex: 0 };
        _this.prevPkg = props.pkg;
        return _this;
    }
    VersionContainer.prototype.onVersionChange = function (event) {
        this.setState({ selectedVerIndex: event.target.value });
    };
    VersionContainer.prototype.onPackageItemClick = function () {
        this.setState({ selectedVerIndex: 0 });
    };
    VersionContainer.prototype.render = function () {
        if (this.prevPkg != this.props.pkg) {
            // Reset selectedVerIndex in states and refresh the section
            this.onPackageItemClick();
            this.prevPkg = this.props.pkg;
            return React.createElement("div", null);
        }
        var versions = null;
        if (this.props.pkg.versions) {
            versions = this.props.pkg.versions;
        }
        var selectedVersion = versions[this.state.selectedVerIndex];
        var change_log = selectedVersion.change_log;
        var content = selectedVersion.content;
        var deps = [];
        for (var i = 0; i < selectedVersion.direct_dependency_ids.length; i++) {
            deps.push({
                name: selectedVersion.direct_dependency_ids[i].name,
                version: selectedVersion.direct_dependency_versions[i]
            });
        }
        var versionDetailView = React.createElement(VersionDetailView_1.VersionDetailView, { changeLog: change_log, content: content, dependencies: deps });
        var options = [];
        if (versions) {
            for (var i_1 = versions.length - 1; i_1 >= 0; i_1--) {
                var index = versions.length - 1 - i_1;
                options.push(React.createElement("option", { value: index }, versions[i_1].version));
            }
        }
        return (React.createElement("div", { className: "VersionContainer" },
            React.createElement("div", { className: "DetailSectionHeader" }, "Versions"),
            React.createElement("div", { className: "VersionBar" },
                React.createElement("div", { className: "VersionBarInfo" },
                    React.createElement("div", { className: "VersionBarColumn" }, this.props.pkg.name),
                    React.createElement("div", { className: "VersionBarColumn" },
                        React.createElement("select", { name: "versions", onChange: this.onVersionChange.bind(this) }, options)),
                    React.createElement("div", { className: "VersionBarColumn" },
                        "Works with Dynamo ",
                        versions == null ? "" : versions[this.state.selectedVerIndex].engine_version)),
                React.createElement(InstallButtons_1.InstallButtons, { pkgController: this.props.pkgController, packageLink: "", packageName: "Foo", packageVersion: "0.0.0" })),
            versionDetailView));
    };
    return VersionContainer;
}(React.Component));
exports.VersionContainer = VersionContainer;


/***/ }),
/* 16 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var VersionDetailView = (function (_super) {
    __extends(VersionDetailView, _super);
    function VersionDetailView(detail) {
        return _super.call(this, detail) || this;
    }
    VersionDetailView.prototype.render = function () {
        var deps = this.props.dependencies.map(function (dep) { return React.createElement("li", null,
            dep.name,
            React.createElement("span", null, "\u00A0\u00A0"),
            "(",
            dep.version,
            ")"); });
        return (React.createElement("div", { className: "VersionDetailView" },
            React.createElement("div", { className: "DetailSectionHeader" }, "What's New"),
            React.createElement("div", { className: "DetailSection" }, this.props.changeLog),
            React.createElement("div", { className: "DetailSectionHeader" }, "Known Issues"),
            React.createElement("div", { className: "DetailSection" }),
            React.createElement("div", { className: "DetailSectionHeader" }, "Dependencies"),
            React.createElement("div", { className: "DetailSection" }, deps),
            React.createElement("div", { className: "DetailSectionHeader" }, "Package Contents"),
            React.createElement("div", { className: "DetailSection" }, this.props.content)));
    };
    ;
    return VersionDetailView;
}(React.Component));
exports.VersionDetailView = VersionDetailView;


/***/ }),
/* 17 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var React = __webpack_require__(0);
var ReactDOM = __webpack_require__(2);
var PackageController_1 = __webpack_require__(6);
var EventHandler_1 = __webpack_require__(1);
var TabComponent_1 = __webpack_require__(8);
var PackageDetailView_1 = __webpack_require__(7);
var TabControl = (function () {
    function TabControl(config) {
        this.tabCaptions = [];
        this.tabContents = [];
        this.reactor = null;
        this.reactor = new EventHandler_1.Reactor();
        this.insertTab = this.insertTab.bind(this);
        this.on = this.on.bind(this);
        this.raiseEvent = this.raiseEvent.bind(this);
        var htmlElement = document.getElementById(config.htmlElementId);
        ReactDOM.render(React.createElement(TabComponent_1.TabComponent, { pkgController: config.pkgController, tabControl: this }), htmlElement);
    }
    TabControl.prototype.insertTab = function (caption, content) {
        this.tabCaptions.push(caption);
        this.tabContents.push(content);
    };
    TabControl.prototype.on = function (eventName, callback) {
        this.reactor.registerEvent(eventName, callback);
    };
    TabControl.prototype.raiseEvent = function (name, params) {
        this.reactor.raiseEvent(name, params);
    };
    return TabControl;
}());
exports.TabControl = TabControl;
var DetailedView = (function () {
    function DetailedView(config) {
        this.reactor = null;
        this.pkgController = null;
        this.showClose = false;
        this.htmlElementId = config.htmlElementId;
        this.reactor = new EventHandler_1.Reactor();
        this.pkgController = config.pkgController;
        if (config.showCloseButton) {
            this.showClose = config.showCloseButton;
        }
        this.setActivePackageId = this.setActivePackageId.bind(this);
        this.on = this.on.bind(this);
        this.raiseEvent = this.raiseEvent.bind(this);
    }
    DetailedView.prototype.setActivePackageId = function (packageId) {
        var htmlElement = document.getElementById(this.htmlElementId);
        ReactDOM.render(React.createElement(PackageDetailView_1.PackageDetailView, { pkgController: this.pkgController, packageId: packageId, showCloseButton: this.showClose }), htmlElement);
    };
    DetailedView.prototype.on = function (eventName, callback) {
        this.reactor.registerEvent(eventName, callback);
    };
    DetailedView.prototype.raiseEvent = function (name, params) {
        this.reactor.raiseEvent(name, params);
    };
    return DetailedView;
}());
exports.DetailedView = DetailedView;
function CreatePackageController() {
    return new PackageController_1.PackageController();
}
exports.CreatePackageController = CreatePackageController;


/***/ }),
/* 18 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
// 
function downloadFile(url) {
}
exports.downloadFile = downloadFile;


/***/ })
/******/ ]);
//# sourceMappingURL=bundle.js.map