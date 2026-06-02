// WealthFlow Global JavaScript Utilities

window.animateCards = function () {
    const cards = document.querySelectorAll('.glass-card:not(.skeleton-card)');
    cards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(10px)';
        card.style.transition = 'opacity 0.35s ease, transform 0.35s ease, border-color 0.3s ease, box-shadow 0.3s ease';
        
        setTimeout(() => {
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 45);
    });
};

document.addEventListener("DOMContentLoaded", function () {
    window.animateCards();
});
