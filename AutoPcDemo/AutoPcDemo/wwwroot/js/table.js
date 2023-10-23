function LoadTableContents(tableId, searchId)
{
	$.ajax
	({
		url: "/TestDatas/LoadTable",
		data: { 'searchBy': $("#" + searchId).val() },
		success: function (value)
		{
			if(value) 
			{
				$("#" + tableId + " tbody").remove();
				$("#" + tableId + " thead").remove();
				$("#" + tableId).append(value);
			}
		},
	});
}

function Search(event, tableId, searchId)
{
	if(event.key === 'Enter')
	{
		LoadTableContents(tableId, searchId);
	}
}
