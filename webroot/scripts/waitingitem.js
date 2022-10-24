
function WaitingItem(timeout, onResult) {
    this.text = null;
    this.onResult = onResult;
    this.cancelSelect = false;
    this.timeout = timeout;

    this.waitingOptions = {
        lines: 11, // The number of lines to draw
        length: 3, // The length of each line
        width: 5, // The line thickness
        radius: 12, // The radius of the inner circle
        corners: 1, // Corner roundness (0..1)
        rotate: 0, // The rotation offset
        direction: 1, // 1: clockwise, -1: counterclockwise
        color: '#000', // #rgb or #rrggbb or array of colors
        speed: 1, // Rounds per second
        trail: 60, // Afterglow percentage
        shadow: false, // Whether to render a shadow
        hwaccel: true, // Whether to use hardware acceleration
        className: 'spinner', // The CSS class to assign to the spinner
        zIndex: 2e9, // The z-index (defaults to 2000000000)
        top: '50%', // Top position relative to parent
        left: '50%' // Left position relative to parent
    };
};

WaitingItem.prototype.show = function() {
    $("#shader").show();
    $("#shader").animate({opacity: 0.5}, 300);
    var target = $("body").get(0);
    this.spinner = new Spinner(this.waitingOptions).spin(target);
    
    var self = this;

    this.timeouter = setTimeout(function() {

//        $("#shader").animate({opacity: 0}, 800, function() {
//            $("#shader").hide();
//        });

        self.spinner.stop();
        self.onResult("timeout");
    }, self.timeout);
};

WaitingItem.prototype.stop = function() {
    this.spinner.stop();
    if (this.timeouter) {
        clearTimeout(this.timeouter);
        this.timeouter = null;
    }
};

var timeout;