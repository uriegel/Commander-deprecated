
function ItemsCollection(nodeFactoryId) {
    this.items;
    this.itemsView;
    this.tableView;
    this.currentDirectory;
    this.recentDirectories = new Array;
    this.itemNodeFactory = $(nodeFactoryId)[0];
}

ItemsCollection.prototype.initialize = function(tableView) {
    this.tableView = tableView;
};

ItemsCollection.prototype.onDirectoryChanged = function(callback) {
    this.onDirectoryChangedCallback = callback;
};

ItemsCollection.prototype.changeNodeFactory = function(nodeFactoryId) {
    this.itemNodeFactory = $(nodeFactoryId)[0];
};

ItemsCollection.prototype.processItem = function(itemIndex, openWith) {
    var dir;
    var lastCurrentDir;
    switch (this.itemsView[itemIndex].__type) {
        case "DriveItem:#":
            dir = this.itemsView[itemIndex].name;
            this.recentDirectories.push(dir);
            break; 
        case "ParentItem:#":
            dir = this.itemsView[itemIndex].parent;
            lastCurrentDir = this.recentDirectories.pop();
            break;
        case "DirectoryItem:#":
            dir = pathCombine(this.items.currentDirectory, this.itemsView[itemIndex].name);
            this.recentDirectories.push(this.itemsView[itemIndex].name);
            break;
        case "FileItem:#":
            this.tableView.process(this.currentDirectory + '\\' + this.itemsView[itemIndex].name, openWith);
            return;
        default:
            return;
    }
    
    this.tableView.clearItems();
    this.getItems(dir, true, lastCurrentDir);
};

ItemsCollection.prototype.getSelectedItems = function(currentItemIndex) {
    var result = new Array;
    this.itemsView.forEach(function(item) {
        if (item.selected)
            result.push(item);
    });
    if (result.length == 0)
        result.push(this.itemsView[currentItemIndex]);
    return result;
};

ItemsCollection.prototype.refresh = function(setFocus) {
    this.tableView.clearItems();
    this.getItems(this.currentDirectory, setFocus);
};


ItemsCollection.prototype.getItems = function(directory, setFocus, lastCurrentDir) {
    if (this.currentDirectory != directory) {
        if (this.onDirectoryChangedCallback)
            this.onDirectoryChangedCallback(directory);
    }
        
    this.currentDirectory = directory;
    var self = this;
    $.getJSON("/Commander/GetItems", {
            dir: directory
        },
        function(json) {
            $.getJSON("/Commander/GetExtendedInfos", {
                    id: json.resultID
                },
                function(json) {
                    var refresh = false;
                    json.items.forEach(function(item) {
                        if (item.version)
                            self.items.items[item.index].version = item.version; 
                        if (item.dateTimeString)
                            self.items.items[item.index].exifDateTime = item.dateTimeString; 
                        refresh = true;
                    });    
                    if (refresh)
                        self.tableView.refreshItems();
            });
            
            self.items = json;
            var lastCurrentIndex;
            for (var i = 0; i < self.items.items.length; i++) {
                var currentItem = self.items.items[i];
                if (currentItem.__type != "DriveItem:#" && currentItem.__type != "DirectoryItem:#")
                    continue;
                if (currentItem.name == lastCurrentDir) {
                    lastCurrentIndex = i;
                    break;
                }
            }
            self.itemsView = self.items.items;
            self.tableView.onItemsChanged(setFocus, lastCurrentIndex);
        }
    );
};

ItemsCollection.prototype.getItemsCount = function() {
    return this.itemsView.length;
};

ItemsCollection.prototype.restrict = function(prefix, back) {
    var restrictedItems = new Array;
    if (back)
        this.itemsView = this.items.items;
    this.itemsView.forEach(function(item) {   
        if (item.__type == "FileItem:#" || item.__type == "DirectoryItem:#" || item.__type == "ServiceItem:#") {
            if (item.name.toLowerCase().indexOf(prefix) == 0) 
                restrictedItems.push(item);
        }
    });
    
    if (restrictedItems.length > 0) {
        this.itemsView = restrictedItems;
        this.tableView.clearItems();
        this.tableView.onItemsChanged(true, null, true);
        return true;
    }
    return false;
};

ItemsCollection.prototype.closeRestrict = function() {
    restrictedItems = null;
    this.itemsView = this.items.items;
    this.tableView.clearItems();
    this.tableView.onItemsChanged(true, null, true);
};

ItemsCollection.prototype.insertItem = function(itemIndex) {
    var item = this.itemsView[itemIndex];
    switch (item.__type) {
        case "DriveItem:#":
            return this.insertDriveItem(item);
        case "ParentItem:#":
            return this.insertParentItem(item);
        case "DirectoryItem:#":
            return this.insertDirectoryItem(item);
        case "FileItem:#":
            return this.insertFileItem(item);
        case "ServiceItem:#":
            return this.insertServiceItem(item);
    }
};

ItemsCollection.prototype.insertDriveItem = function(drive) {
    var $node = $(this.itemNodeFactory.cloneNode(true));
    $node.find(".image").attr("src", drive.imageUrl);
    $node.find("#name").html(drive.name);
    return $node;
};

ItemsCollection.prototype.insertServiceItem = function(service) {
    var $node = $(this.itemNodeFactory.cloneNode(true));
    $node.find(".image").attr("src", service.imageUrl);
    $node.find("#name").html(service.name);
    $node.find("#status").html(service.status);
    
    switch (service.startType) {
        case 2:
            $node.find("#startType").html("Automatisch");
            break;
        case 3:
            $node.find("#startType").html("Manuell");
            break;
        case 4:
            $node.find("#startType").html("Deaktiviert");
            break;
    }
    
    return $node;
};

ItemsCollection.prototype.insertParentItem = function(directory) {
    var $node = $(this.itemNodeFactory.cloneNode(true));
    $node.find(".image").attr("src", directory.imageUrl);
    $node.find("#name").html(directory.name);
    return $node;
};

ItemsCollection.prototype.insertDirectoryItem = function(directory) {
    var $node = $(this.itemNodeFactory.cloneNode(true));
    $node.find(".image").attr("src", directory.imageUrl);
    $node.find("#name").html(directory.name);
    $node.find("#time").html(directory.dateTimeString);
    if (directory.isHidden)
        $node.addClass("hidden");
    this.insertExtendedInfos($node, directory);
    return $node;
};

ItemsCollection.prototype.insertFileItem = function(file) {
    var $node = $(this.itemNodeFactory.cloneNode(true));
    $node.find(".image").attr("src", file.imageUrl);
    $node.find("#name").html(this.getNameOnly(file.name));
    $node.find("#extension").html(this.getExtension(file.name));
    $node.find("#size").html(this.numberFormat(file.fileSize));
    $node.find("#time").html(file.dateTimeString);
    if (file.isHidden)
        $node.addClass("hidden");
    this.insertExtendedInfos($node, file);
    return $node;
};

ItemsCollection.prototype.refreshServiceItem = function(serviceItem) {
    this.items.items.forEach(function(item) {
        if (item.name == serviceItem.name) {
            item.status = serviceItem.status;
            item.imageUrl = serviceItem.imageUrl; 
        }
    });
    this.tableView.refreshItems();
};

ItemsCollection.prototype.refreshItem = function($item, index) {
      this.insertExtendedInfos($item, this.itemsView[index]);
};

ItemsCollection.prototype.insertExtendedInfos = function($item, item) {
    if (item.version)
        $item.find("#version").html(item.version);
    if (item.exifDateTime) {
        var $time = $item.find("#time");
        $time.html(item.exifDateTime);
        $time.addClass("exif");
    }
    if (item.selected) 
        $item.addClass("selected");
    else
        $item.removeClass("selected");
};


ItemsCollection.prototype.numberFormat = function(number) {
    var strNumber = number+"";
    var thSep = '.';
    if(strNumber.length > 3) {
        var begriff = strNumber;
        strNumber = "";
        for(j = 3; j < begriff.length ; j+=3) {
            var extract = begriff.slice(begriff.length - j, begriff.length - j + 3);
            strNumber = thSep + extract +  strNumber + "";
        }
        var strfirst = begriff.substr(0, (begriff.length % 3 == 0) ? 3 : (begriff.length % 3));
        strNumber = strfirst + strNumber;
    }
    return strNumber;
};

ItemsCollection.prototype.toggleSelection = function($item, itemIndex) {
    if (!this.canBeSelected(itemIndex))
        return;
    
    if (this.itemsView[itemIndex].selected) 
        this.itemsView[itemIndex].selected = false;
    else
        this.itemsView[itemIndex].selected = true;
    this.refreshItem($item, itemIndex);
};

ItemsCollection.prototype.selectAll = function(select, startIndex) {
    var self = this;
    this.itemsView.forEach(function(item, index) {
        if (self.canBeSelected(index)) {
            if (!startIndex || index >= startIndex) 
                self.itemsView[index].selected = select;
            else
                self.itemsView[index].selected = !select;
        }
    });
};

ItemsCollection.prototype.canBeSelected = function(itemIndex) {
    switch (this.itemsView[itemIndex].__type) {
        case "DirectoryItem:#":
            return true;
        case "FileItem:#":
            return true;
        default:
            return false;
    }   
};

ItemsCollection.prototype.getNameOnly = function(name) {
    var pos = name.lastIndexOf('.');
    if (pos == -1)
        return name;
    return name.substring(0, pos);
};

ItemsCollection.prototype.getExtension = function(name) {
    var pos = name.lastIndexOf('.');
    if (pos == -1)
        return null;
    return name.substring(pos);
};

ItemsCollection.prototype.getCurrentDirectory = function() {
    return this.items ? this.items.currentDirectory : null;
};

ItemsCollection.prototype.getItemName = function(itemIndex) {
    if (!this.itemsView)
        return null;
    return this.itemsView[itemIndex].name;
};

function pathCombine(path1, path2) {
    if (path1.charAt(path1.length - 1) == '\\')
        return path1 + path2;
    return path1 + '\\' + path2;
}