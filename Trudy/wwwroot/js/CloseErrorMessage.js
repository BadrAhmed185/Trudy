document.addEventListener('DOMContentLoaded', function () {

    function closeMessage(button) {
        const message = button.closest('.error-message, .success-message');
        message.classList.add('hide');
        setTimeout(() => {
            message.remove();
        }, 300);
    }

    const messages = document.querySelectorAll('.error-message, .success-message');
    messages.forEach(msg => {
        setTimeout(() => {
            msg.classList.add('hide');
            setTimeout(() => {
                msg.remove();
            }, 300);
        }, 5000); // auto-dismiss after 5 seconds
    });

});