
$(document).ready(function () {

    // set the default edit language to the current langauge
    $('#editlang').val($('#selectparams #lang').val());

    // get list of records via ajax:  NBrightRazorTemplate_nbxget({command}, {div of data passed to server}, {return html to this div} )
    NBrightBuyOpenUrlRewriter_nbxget('getdata', '#selectparams', '#editdata');

    $('.actionbuttonwrapper #cmdsave').click(function () {
        NBrightBuyOpenUrlRewriter_nbxget('savedata', '#editdata');
    });

    $('.selecteditlanguage').click(function () {
        $('#editlang').val($(this).attr('lang')); // alter lang after, so we get correct data record
        NBrightBuyOpenUrlRewriter_nbxget('selectlang', '#editdata'); // do ajax call to save current edit form
    });


});

$(document).on("NBrightBuyOpenUrlRewriter_nbxgetcompleted", NBrightBuyOpenUrlRewriter_nbxgetCompleted); // assign a completed event for the ajax calls


function NBrightBuyOpenUrlRewriter_nbxget(cmd, selformdiv, target, selformitemdiv, appendreturn) {
    $('.processing').show();

    $.ajaxSetup({ cache: false });

    var cmdupdate = '/DesktopModules/NBright/NBrightBuyOpenUrlRewriter/XmlConnector.ashx?cmd=' + cmd;
    var values = '';
    if (selformitemdiv == null) {
        values = $.fn.genxmlajax(selformdiv);
    }
    else {
        values = $.fn.genxmlajaxitems(selformdiv, selformitemdiv);
    }
    var request = $.ajax({
        type: "POST",
        url: cmdupdate,
        cache: false,
        data: { inputxml: encodeURI(values) }
    });

    request.done(function (data) {
        if (data != 'noaction') {
            if (appendreturn == null) {
                $(target).children().remove();
                $(target).html(data).trigger('change');
            } else
                $(target).append(data).trigger('change');

            $.event.trigger({
                type: "NBrightBuyOpenUrlRewriter_nbxgetcompleted",
                cmd: cmd
            });
        }
        if (cmd == 'getdata') { // only hide on getdata
            $('.processing').hide();
        }
    });

    request.fail(function (jqXHR, textStatus) {
        alert("Request failed: " + textStatus);
    });
}



function NBrightBuyOpenUrlRewriter_nbxgetCompleted(e) {
    $('.processing').hide();
}



