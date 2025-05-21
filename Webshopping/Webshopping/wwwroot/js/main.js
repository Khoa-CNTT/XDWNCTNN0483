/* price range */
$("#sl2").slider && $("#sl2").slider();

var RGBChange = function () {
  $("#RGB").css(
    "background",
    "rgb(" + r.getValue() + "," + g.getValue() + "," + b.getValue() + ")"
  );
};

/* scroll to top */
$(document).ready(function () {
  $.scrollUp &&
    $.scrollUp({
      scrollName: "scrollUp",
      scrollDistance: 300,
      scrollFrom: "top",
      scrollSpeed: 300,
      easingType: "linear",
      animation: "fade",
      animationSpeed: 200,
      scrollText: '<i class="fa fa-chevron-up"></i>',
      zIndex: 2147483647,
    });

  /* bảo hiểm – có thể bỏ nếu data-ride đã kích hoạt */
  $("#slider-carousel").carousel({
    interval: 5000,
    pause: "hover",
  });
});
