document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('formMagang');
    if (!form) return;

    form.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {

            if (e.target.tagName.toLowerCase() === 'textarea') return;

            e.preventDefault();

            const currentStepElement = document.querySelector('.step-content.active');
            if (!currentStepElement) return;

            const focusableElements = currentStepElement.querySelectorAll(
                'input:not([type="hidden"]):not([disabled]), select:not([disabled]), textarea:not([disabled]), button.btn-next'
            );

            const index = Array.from(focusableElements).indexOf(e.target);

            if (index > -1) {
                const nextElement = focusableElements[index + 1];

                if (nextElement) {
                    nextElement.focus();
                } else {
                    const nextBtn = currentStepElement.querySelector('.btn-next');
                    if (nextBtn) nextBtn.click();
                }
            }
        }
    });
});