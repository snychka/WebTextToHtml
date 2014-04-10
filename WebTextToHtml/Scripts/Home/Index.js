// Copyright Stefan Nychka, BSD 3-Clause license, COPYRIGHT.txt
/// <reference path="../jquery-1.8.2.intellisense.js" />

var DataFromView = {
    init: function 
    (
        convertText,
        textAreaId,
        retrieveText,
        formSelector,
        retrieveActionUrl
    )
    {
        this.convertText = convertText;
        this.textAreaId = textAreaId;
        this.retrieveText = retrieveText;
        this.formSelector = formSelector;
        this.retrieveActionUrl = retrieveActionUrl;
    }
};

$(function () {

    var textArea = $(DataFromView.textAreaId);
    var oldVal = textArea.val();
    var button = $('input[name="' + DataFromView.convertText + '"');

    if (oldVal != '') { // page loaded with data
        enableButton(button);
    }

    // "un-disables" convert button when text is entered.
    // eventually just try change
    textArea.on("change keyup paste", function () {
        var currentVal = $(this).val();
        // helps mitigate unnecessary code execuction
        if (currentVal === oldVal) {
            return;
        }
        oldVal = currentVal;
        if (currentVal != "") {
            enableButton(button);
        } else {
            disableButton(button);
        }
    });

    // assumes button in html starts out as disabled
    function enableButton(button) {
        button.removeAttr("disabled");
        button.removeClass("disabled");
    }

    function disableButton(button) {
        button.attr("disabled", "true");
        button.addClass("disabled");
    }

    $('input[name="' + DataFromView.retrieveText + '"]').on('click', function (event) {
        event.preventDefault();
        $(DataFromView.formSelector).attr('action', DataFromView.retrieveActionUrl);
        $(DataFromView.formSelector).submit();
    });

});
