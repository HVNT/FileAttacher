function MainViewModel() {

    var self = this;
    self.files = ko.observableArray();

    /* For making trash can appear on hover
    self.deleteEnabled = ko.observable(false);
    self.enableDelete = function () {
        self.deleteEnabled(true);
    }
    self.disableDelete = function () {
        self.deleteEnabled(false);
    }
    */
    $.ajax({
        type: "GET",
        contentType: "application/json",
        dataType: "json",
        url: "/api/v1/FileAtt/GetAll",
        success: function (data) {
            console.log(data);
            self.files(data);
        },
        error: function (data) {
            console.log(data);
        }
    });

    self.removeFile = function (file) {

        var f = file;

        $.ajax({
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            url: "/api/v1/FileAtt/RemoveS3File?f=" + f.Id,
            success: function (data) {
                console.log(data);
                location.reload();
            },
            error: function (data) {
                console.log(data);
            }
        });
    }
}