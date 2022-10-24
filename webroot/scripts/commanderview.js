
function CommanderView(id) {
    this.commanderCollection = new ItemsCollection("#templates .item");
    this.commanderCollection.onDirectoryChanged(this.directoryChanged.bind(this));
   
    this.tableView = new TableView(id + " .tableView", this.commanderCollection);

    this.directoryHeight = 20;
    this.$restrict = null;
    $("#" + id + " .restrict").hide();

    var self = this;
    this.tableView.onItemsChangedCallback = function(directory) {
        self.onItemsChangedCallback(directory);
    };
    this.tableView.onProcess = function(path, openWith) {
        self.onProcessCallback(path, openWith);
    };
    this.id = id;
    this.$commanderView = $("#" + id);
    this.$directory = $("#" + id + " .directory");
    this.$directory.change(function() {
        self.changeDirectory($(this).val());
    });
    this.$directory.focus(function() {
        window.setTimeout (function(){ 
            self.$directory.select(); 
        },100);
    });
    this.$directory.keydown(function(e) {
        switch (e.which) {
            case 13:
                self.tableView.clearItems();
                self.tableView.itemsCollection.getItems($(this).val(), true);
                break;
        }
    });
    this.tableView.onKeydown = function(e) {
        switch (e.which) {
            case 27:
                self.closeRestrict();
                break;
            case 107:
                self.tableView.selectAll(true);
                break;
            case 109:
                self.tableView.selectAll(false);
                break;
            case 36:
                if (e.shiftKey) {
                    self.tableView.selectAll(false, self.tableView.currentItem + 1);
                    e.preventDefault();
                }
                break;
            case 35:
                if (e.shiftKey) {
                    self.tableView.selectAll(true, self.tableView.currentItem);
                    e.preventDefault();
                }
                break;
            case 82:
                if (e.ctrlKey) {
                    self.refresh(true);
                    break;
                }
                else
                    self.keysRestrict(e);
                break;
            case 83:
                self.keysRestrict(e);
                break;
            case 32:
                if (self.$restrict == null)
                    self.tableView.toggleSelection();
                else
                    self.keysRestrict(e);
                break;
            case 8:
                if (self.$restrict != null) {
                    self.restrictBack();
                    e.preventDefault();
                    return;
                }
            case 45:
                self.tableView.toggleSelection();
                self.tableView.downOne();
                break;

            default:
                if (e.char)
                    self.keysRestrict(e);
        }
    };
    this.tableView.onSelected(function(itemIndex, openWith) {
        self.commanderCollection.processItem(itemIndex, openWith);
    });
    this.tableView.onCurrentItemChanged(function(itemIndex) {
        self.onCurrentItemChangedCallback(self.commanderCollection.getItemName(itemIndex));
    });
}

CommanderView.prototype.initialize = function(settings) {
    this.settings = settings;
    this.columns = new ColumnsControl([{item: "Name"}, {item: "Erw."}, {item: "Größe", class: "size"}, {item: "Datum"}, {item: "Version"}]);
    this.tableView.setColumns(this.columns);
    this.columns.restoreSettings(settings.columns, this.tableView.getWidth());
    this.getItems(settings.directory, true);
};

CommanderView.prototype.getSettings = function() {
    var columns;
    var serviceColumns;
    if (this.columns == this.tableView.columnsControl) {
        columns = this.columns.getSettings();
        serviceColumns = this.settings.serviceColumns;
    }
    else {
        columns = this.settings.columns;
        serviceColumns = this.columns.getSettings();
    }
    return {
        directory: this.getCurrentDirectory(),
        columns: columns,
        serviceColumns: serviceColumns
    };
};

CommanderView.prototype.changeDirectory = function(directory) {
    this.tableView.clearItems();
    this.tableView.itemsCollection.getItems(directory, true);
};

CommanderView.prototype.onCurrentItemChanged = function(callback) {
    this.onCurrentItemChangedCallback = callback;
};

CommanderView.prototype.onProcess = function(callback) {
    this.onProcessCallback = callback;
};

    CommanderView.prototype.getItems = function(directory, focus) {
    this.commanderCollection.getItems(directory, focus);
};

CommanderView.prototype.checkRestrict = function(restrict) {
    if (this.$restrict == null) {
        this.$restrict = $("#" + this.id + " .restrict");
        this.$restrict.css("top", this.height - 25 - 18);
        this.$restrict.show("slide");
    }
    this.$restrict.val(restrict);
};

CommanderView.prototype.closeRestrict = function() {
    if (this.$restrict != null) {
        this.$restrict.hide("slide");
        this.$restrict = null;
        this.tableView.itemsCollection.closeRestrict();
    }
};

CommanderView.prototype.offset = function(offset) {
    this.$commanderView.offset(offset);
    var viewOffset = { left: offset.left, top: offset.top + this.directoryHeight};
    this.tableView.offset(viewOffset);
    
};

CommanderView.prototype.resize = function(width, height) {
    this.height = height;
    this.$directory.width(width - 3);
    this.tableView.resize(width, height - this.directoryHeight);
    if (this.restrict)
        this.restrict.offset({top: height - 25});
};

CommanderView.prototype.onItemsChangedCallback = function(directory) {
    if (directory == "Dienste")
        this.tableView.itemsCollection.changeNodeFactory("#templates .serviceItem");
    else
        this.tableView.itemsCollection.changeNodeFactory("#templates .item");
    this.$directory.val(directory);
    this.closeRestrict();
};

CommanderView.prototype.hasFocus = function() {
    return this.tableView.hasFocus();
};

CommanderView.prototype.focus = function() {
    this.tableView.focus();
    this.onCurrentItemChangedCallback(this.commanderCollection.getItemName(this.tableView.getCurrentItemIndex()));
};

CommanderView.prototype.getCurrentDirectory = function() {
    return this.tableView.itemsCollection.getCurrentDirectory();
};

CommanderView.prototype.getWidths = function() {
    return this.tableView.columnsControl.getWidths();
};

CommanderView.prototype.refresh = function(setFocus) {
    this.tableView.refresh(setFocus);
};

CommanderView.prototype.getSelectedItems = function() {
    var currentItem = this.tableView.getCurrentItemIndex();
    return this.commanderCollection.getSelectedItems(currentItem);
};

CommanderView.prototype.keysRestrict = function(e) {
    var restrict = e.char.toLowerCase();
    if (this.$restrict != null)
        restrict = this.$restrict.val() + restrict;

    if (this.tableView.itemsCollection.restrict(restrict))
        this.checkRestrict(restrict);
};

CommanderView.prototype.restrictBack = function() {
    var restrict = this.$restrict.val();
    restrict = restrict.substring(0, restrict.length - 1);
    if (restrict.length == 0)
        this.closeRestrict();
    else {
        if (this.tableView.itemsCollection.restrict(restrict, true))
            this.checkRestrict(restrict);
    }
};

CommanderView.prototype.saveView = function() {
    var currentItem = this.tableView.getCurrentItemIndex();
    var selectedItems = this.commanderCollection.getSelectedItems(currentItem);
    if (selectedItems && selectedItems.length > 20)
        selectedItems = null;
    return {
        directory: this.commanderCollection.getCurrentDirectory(),
        position: currentItem,
        selectedItems: selectedItems
    };
};

CommanderView.prototype.directoryChanged = function(newDirectory) {
    if (newDirectory == "Dienste") {
        if (!this.serviceColumns)
            this.serviceColumns = new ColumnsControl([{item: "Name"}, {item: "Status"}, {item: "Startart"}]);
        this.settings.columns = this.columns.getWidths();
        this.tableView.setColumns(this.serviceColumns);
        this.serviceColumns.restoreSettings(this.settings.serviceColumns, this.tableView.getWidth());
    }
    else {
        if (this.tableView.columnsControl == this.serviceColumns) {
            this.settings.serviceColumns = this.columns.getWidths();
            this.tableView.setColumns(this.columns);
            this.columns.restoreSettings(this.settings.columns, this.tableView.getWidth());
        }
    }
};

CommanderView.prototype.refreshServiceItem = function(serviceItem) {
    if (this.columns == this.tableView.columnsControl) {
        return;
    }    

    this.commanderCollection.refreshServiceItem(serviceItem);
};
