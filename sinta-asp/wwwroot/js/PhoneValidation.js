$(document).ready(function () {
    $('#no_hp').on('input', function () {
        let value = $(this).val().replace(/\D/g, '');
        $(this).val(value);

        const errorMsg = $('#hp-error');

        if (value.length >= 2 && value.substring(0, 2) !== '08') {
            $(this).addClass('is-invalid');
            errorMsg.css('display', 'flex');
        } else {
            $(this).removeClass('is-invalid');
            errorMsg.hide();
        }
    });
});
