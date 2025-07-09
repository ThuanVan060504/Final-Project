function toggleMomoButton() {
    const checkbox = document.getElementById('momoConfirm');
    const button = document.getElementById('momoBtn');
    button.style.display = checkbox.checked ? 'inline-block' : 'none';
}

function toggleBankButton() {
    const checkbox = document.getElementById('bankConfirm');
    const button = document.getElementById('bankBtn');
    button.style.display = checkbox.checked ? 'inline-block' : 'none';
}
