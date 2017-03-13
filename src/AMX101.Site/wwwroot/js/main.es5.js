'use strict';

$(document).ready(function () {
    $('body').on('click', '.toggleRowsView', function () {
        $('.secondTileRow').toggle();
        $('.altView').toggleClass('active');
        $(this).find('.fa').toggle();
    });
    var showHideFooter = function showHideFooter() {
        $('.viewTermsAndConditions').on('click', function () {
            $('.expandedTermsAndConditions').toggle('slide', {
                direction: 'down'
            }, 500);

            $('.viewTermsAndConditions').find('.fa').toggle();

            $('.fullScreenOverlay').fadeToggle();
        });
    };

    var toggleRowsView = function toggleRowsView() {
        // console.log("hello");
        var headerHeight = $('.headerHeight').outerHeight();

        // Not 100% sure why the below code was place in the javascript, but it was causing strange heights on the results view so i have removed it
        // Joshua.Sansom ---> Speak to me if this is causing issues
        // var headerHeight = $('.headerHeight').outerHeight();

        // $('.headerHeight').css({
        //     'height':headerHeight
        // });
    };

    //showHideFooter();
    //toggleRowsView();
});

