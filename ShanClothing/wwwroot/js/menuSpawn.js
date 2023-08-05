var hamb = document.getElementById('hamb');
var menu = document.getElementById('menu-spawn');

hamb.addEventListener('click', () => {
    if(getComputedStyle(menu).display == 'none')
    {
        menu.style.display = "block";
    }
    else
    {
        menu.style.display = "none";
    }
})