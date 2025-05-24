$(document).ready(function () {
  $(".delete-form").on("submit", function (e) {
    e.preventDefault(); // Ngăn submit mặc định

    const form = this;

    Swal.fire({
      title: "Bạn có chắc muốn xóa?",
      text: "Hành động này không thể hoàn tác!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Vâng, xóa đi!",
      cancelButtonText: "Hủy bỏ",
    }).then((result) => {
      if (result.isConfirmed) {
        form.submit();
      }
    });
  });
});

// thẻ a order
$(document).ready(function () {
  $(".btn-delete").on("click", function (e) {
    e.preventDefault();
    const url = $(this).data("url");

    Swal.fire({
      title: "Bạn có chắc muốn xóa đơn hàng này?",
      text: "Hành động này không thể hoàn tác.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Vâng, xóa đi!",
      cancelButtonText: "Hủy",
    }).then((result) => {
      if (result.isConfirmed) {
        window.location.href = url; // chuyển hướng đến action Delete
      }
    });
  });
});
