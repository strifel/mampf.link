// Function is used in Group overview page to copy sharetext
function copySharetext(menu_url, group_link, closingTime) {
    if (navigator.language === "de") {
        navigator.clipboard.writeText(
            'Wir bestellen Essen🥘🍲🍕! \n' +
            (menu_url ? 'Link zur Speisekarte: ' + menu_url + ' \n' : '') + 
            'Füge deine Bestellungen unter ' + group_link + ' hinzu.\n' +
            'Bestellung bis ' + closingTime + '.'
        )
    } else {
        navigator.clipboard.writeText(
            'We are ordering food🥘🍲🍕! \n' +
            (menu_url ? 'Link to menu: ' + menu_url + ' \n' : '') +
            'Add your order here: ' + group_link + ' \n' +
            'Taking orders until ' + closingTime
        )
    }
    
}