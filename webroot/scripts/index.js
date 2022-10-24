// Im JSON des Dialogs OriginalSelection durchreichen
// Erst Dienst nach allgemeinen Daten fragen (Kopieren, Verschieben von wo nach wo, geht das...
// Dann Dialogbox: Möchtest Du die ausgewählten Dateien von A nach B kopieren/verschieben?
// Dann Konfliktitems holen, dabei drehendes Zahnrad bringen, Aktion nach Timeout abbrechen
// Eventuell fragen, ob als Administrator, wenn keine Zugriffe auf Verzeichnisse gewährleistet werden können
// Dann ggf. KonflikItems untersuchen
// Dann kopieren/verschieben

// Fehler (Kein Zugriff) melden
// ExtendedInfos: DierctoryName überprüfen
// Löschen der nicht mehr benötigten ItemResults im HTTPServer nach gewisser Zeit (1 min), alle 10 min checken
// Suchen nach Elementen, Eingabe eines Zeichens füllt nur die Elemente, die mit dem Zeichen anfangen
// Sortierung nach Spalten
// Variable Spalten, auch mit Alignment pro Spalte 
// Css-Img der Scrollbar verhält sich beim Clicken schlecht

$(document).ready(function() {
    settings = getSettings();
    if (settings.version != "1.1")
        settings = getSettings(true);
    viewLeft = new CommanderView("commanderLeft");
    viewRight = new CommanderView("commanderRight");
    viewLeft.onProcess(processItem);
    viewRight.onProcess(processItem);
    viewLeft.onCurrentItemChanged(onCurrentItemChanged);
    viewRight.onCurrentItemChanged(onCurrentItemChanged);
    if (!settings.savedViews)
        settings.savedViews = new Array;    
    
    var grid = new Grid("grid");
    grid.setLeft(viewLeft);
    grid.setRight(viewRight);
    
    var footerHeight = $("footer").outerHeight();
    grid.resize($(window).width(), $(window).height() - footerHeight);  
    
    viewLeft.initialize(settings.left);
    viewRight.initialize(settings.right);
    
    initializeWebSocket();
    
    $(window).resize(function() {
        grid.resize($(window).width(), $(window).height() - footerHeight);
    });
    
    $(document).keydown(function(e) {
        switch(e.which) {
            case 9: // Tab
                if (e.shiftKey) 
                    controlTab();
                else {
                    inactiveView = getInactiveView();
                    if (inactiveView)
                        inactiveView.focus();
                }
                break;
            case 115: // F4
                if (e.ctrlKey) {
                    if (settings.savedViews.length == 0)
                        return;
                    var deleteIndex = -1;
                    var index = -1;
                    settings.savedViews.forEach(function(item){
                        index++;
                        if (item.left.directory == viewLeft.getCurrentDirectory() && item.right.directory == viewRight.getCurrentDirectory())
                            deleteIndex = index;
                    });
                    if (deleteIndex != -1) 
                        settings.savedViews.splice(deleteIndex, 1);
                    viewLeft.changeDirectory(settings.savedViews[0].left.directory);
                    viewRight.changeDirectory(settings.savedViews[0].right.directory);                    
                }
                break;
            case 116: // F5
                process(getViewsFromElement($(e.target)), getCopyOperationData, operateCopy);
                break;
            case 117: // F6
                process(getViewsFromElement($(e.target)), getMoveOperationData, operateMove);
                break;
            case 120: // F9
                var activeView = getActiveView();
                if (!activeView)
                    return;
                var inactiveView = getInactiveView();
                if (!inactiveView)
                    return;
                var dir = activeView.getCurrentDirectory();
                inactiveView.changeDirectory(dir);
                break;
            case 83: // S
                if (e.ctrlKey) {
                    var view = {
                        left: viewLeft.saveView(),
                        right: viewRight.saveView()
                    };

                    var skip = false;
                    settings.savedViews.forEach(function(item){
                        if (item.left.directory == view.left.directory && item.right.directory == view.right.directory)
                            skip = true;
                    });

                    if (!skip)
                        settings.savedViews.push(view);
                    break;
                }         
                else
                    return;
            case 46:
                process(getViewsFromElement($(e.target)), getDeleteOperationData, operateDelete);
                break;
            default:
                return;
        }        
        e.preventDefault(); // prevent the default action (scroll / move caret)
    });

    $(document).keyup(function(e) {
        if (viewSwitcher && !(e.shiftKey || e.ctrlKey)) {
            viewSwitcher.close();
            var item = viewSwitcher.getSelectedItem();
            viewSwitcher = null;
            if (item[0] == "Abbrechen")
                return;
            viewLeft.changeDirectory(item[0]);
            viewRight.changeDirectory(item[1]);
        }
    });

    $(window).focusin(function() {
        var $active = $(document.activeElement);
        if ($active.length > 0)  {
            var $view = $($active.closest(".CommanderView")[0]);
            var id = $view.attr('id'); 
            if (id == "commanderRight")
                viewFocusFallback = viewRight;
            else if (id == "commanderLeft")
                viewFocusFallback = viewLeft;
        }
        else {
            if (viewFocusFallback)
                viewFocusFallback.focus();
        }
    });
    
    $(window).bind('unload', function() {
        settings.left = viewLeft.getSettings();
        settings.right = viewRight.getSettings();
        localStorage["CommanderSettings"] = JSON.stringify(settings);
    });
});

function controlTab() {
    try {
        if (settings.savedViews.length == 0)
            return;
        if (viewSwitcher) 
            viewSwitcher.switch();
        else {
            viewSwitcher = new ViewSwitcher(getActiveView(), settings.savedViews);
            viewSwitcher.show();
        }
    }
    catch (e){
        alert(e);
    }
}

function initializeWebSocket() {
    if (!webSocket) {
        webSocket = new WebSocket('ws://' + location.host + '/Commander/Events');
        webSocket.onopen = function() {
            webSocket.send(JSON.stringify({ 
                method: "initialize"
            }));
        };
        webSocket.onmessage = function(json) {
            var data = JSON.parse(json.data);
            if (data.method == "copy") {
                process(getViewsFromView(getActiveView()), getCopyOperationData, operateCopy);
            }
            else if (data.method == "move") {
                process(getViewsFromView(getActiveView()), getMoveOperationData, operateMove);
            }
            else if (data.method == "delete") {
                process(getViewsFromView(getActiveView()), getDeleteOperationData, operateDelete);
            }
            else if (data.method == "refresh") {
                getActiveView().refresh(true);
            }
            else if (data.method == "serviceItem") {
                viewLeft.refreshServiceItem(data.serviceItem);
                viewRight.refreshServiceItem(data.serviceItem);
            }
        };
    }
}

function getInactiveView() {
    if (viewLeft.hasFocus())
        return viewRight;
    else if (viewRight.hasFocus())
        return viewLeft;
}

function getActiveView() {
    if (viewLeft.hasFocus())
        return viewLeft;
    else if (viewRight.hasFocus())
        return viewRight;
}

function processItem(path, openWith) {
    var param = { file: path, openWith: openWith };
    $.post("/Commander/Process", JSON.stringify(param));
}

function process(views, getOperationData, operate) {
    var selection = views.active.getSelectedItems();
    if (selection.length == 0)
        return;

    var waitingItem = new WaitingItem(5000, function(result) {
        if (result == "timeout") {
            $.post("/Commander/Cancel", null);
            var dialog = new Dialog(views.active);
            dialog.text = "Dauert zu lange";
            dialog.show();
        }
    });
    waitingItem.show();

    var operationData = getOperationData(views, selection);

    $.post("/Commander/CheckFileOperation", 
        JSON.stringify(operationData),
        function(json) {
            waitingItem.stop();
            var dialog = new Dialog(views.active, json);
            if (json.result == "cancelled") {
            }
            if (json.result == "identicalDirectories") {
                dialog.text = "Die Verzeichnisse sind identisch";
                dialog.show();
            }
            else if (json.result == "noSelection") {
                dialog.text = "Für diese Operation sind keine gültigen Elemente ausgewählt";
                dialog.show();
            }
            else if (json.result == "subordinateDirectory") {
                dialog.text = "Der Zielordner ist dem Quellordner untergeordnet";
                dialog.show();
            }
            else
                operate(json, dialog, views);
        }
    );
}

function getCopyOperationData(views, selection) {
    return {
        operation: "copy",
        sourceDir: views.active.getCurrentDirectory(),
        targetDir: views.inactive.getCurrentDirectory(),
        items: selection
    }; 
}

function getMoveOperationData(views, selection) {
    return {
        operation: "move",
        sourceDir: views.active.getCurrentDirectory(),
        targetDir: views.inactive.getCurrentDirectory(),
        items: selection
    }; 
}

function operateCopy(json, dialog, views) {
    operateFile(json, dialog, views, "Willst Du die ausgewählten Dateien kopieren?", false);
}

function operateMove(json, dialog, views) {
    operateFile(json, dialog, views, "Willst Du die ausgewählten Dateien verschieben?", true);
}

function operateFile(json, dialog, views, question, refreshTarget) {
    if (json.result == "incompatible") {
        dialog.text = "Du kannst die ausgewählten Elemente nicht in diesen Zielordner kopieren";
        dialog.show();
    }
    else if (json.conflictItems.length > 0) {
        dialog.text = "Folgende Dateien überschreiben?";
        dialog.columns = [{item: "Name"}, {item: "Größe", class: "size"} , {item: "Datum"}, {item: "Version"}];
        dialog.itemsCollection = new ConflictCollection("#templates .conflictitem", json);
        dialog.showButtonBar();
        dialog.show(function(ignoreConflict) {
            $.post("/Commander/Operate", 
                JSON.stringify({
                        id: json.id,
                        conflictItems: json.conflictItems,
                        ignoreConflicts: ignoreConflict
                    }), function() {
                        views.inactive.refresh(false);
                            if (refreshTarget)
                                views.active.refresh(false);
                    });
        });
    }
    else {
        dialog.text = question;
        dialog.show(function(action) {
            if (action) {
                $.post("/Commander/Operate", 
                    JSON.stringify({
                            id: json.id
                        }), function() {
                            views.inactive.refresh(false);
                            if (refreshTarget)
                                views.active.refresh(false);
                        });
            }
        });
    }
}

function getDeleteOperationData(views, selection) {
    return {
        operation: "delete",
        sourceDir: views.active.getCurrentDirectory(),
        items: selection
    }; 
}

function operateDelete(json, dialog, views) {
    dialog.text = "Willst Du die ausgewählten Dateien löschen?";
    dialog.show(function(action) {
        if (action) {
            $.post("/Commander/Operate", 
                JSON.stringify({
                        id: json.id
                    }), function() {
                        views.active.refresh(false);
                    });
        }
    });
}

function processResult(json) {
    var wahr = json;
}

function getViewsFromView(view) {
    if (view == viewLeft)
        return { 
            active: viewLeft,
            inactive:  viewRight
        };
    else if (view == viewRight)
        return { 
            active: viewRight,
            inactive:  viewLeft
        };
}

function getViewsFromElement(element) {
    var $view = element.closest(".CommanderView");
    var id = $view.attr("id");
    return getViews(id);
}

function getViews(id) {
    if (id == "commanderLeft") 
        return { 
            active: viewLeft,
            inactive:  viewRight
        };
    else if (id == "commanderRight") 
        return { 
            active: viewRight,
            inactive:  viewLeft
        };
}

function createNewTab() {
    // Auf den CommanderView anstatt auf die tableView zugreifen, da tableview allgemein verwendbar sein soll
    var table = getActiveView();
    table.clearItems();
    table.itemsCollection.getItems("drives", true);
}

function onCurrentItemChanged(itemName) {
    if (this.footerTimer)
        clearInterval(this.footerTimer);
    var self = this;
    this.footerTimer = setInterval(function() {
        clearInterval(self.footerTimer);    
        $("#footer").html(itemName);
    }, 100);
}

function getSettings(forceNew) {
    if (forceNew) {
        localStorage.removeItem("CommanderSettings");
    }
    var json = localStorage["CommanderSettings"];
    if (json) {
        return JSON.parse(json);
    }
    else
        return {
            left: {
                directory: "drives"
            },
            right: {
                directory: "drives"    
            },
            version: "1.1"
        };
}


var viewLeft;
var viewRight;
var viewFocusFallback;
var webSocket;
var viewSwitcher;
var footerTimer;
var settings;