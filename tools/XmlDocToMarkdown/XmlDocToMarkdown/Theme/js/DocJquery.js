$(document).ready(function(){
	$('.docutils').each(function(){
		var testparam = $(this).find("td:contains('Test Param :')").first().parent().prev().index();
		$(this).find('tbody').children().each(function(index){
			if(testparam != -1){
				if (index >=2 && index<=testparam){
					//console.log(index);
					$(this).addClass("params");
					if(index%2 == false){
						$(this).children().addClass("paramstitle");
					}else{
						$(this).children().addClass("paramsdesc");
					}
					if(index==testparam){
						$(this).children().addClass("lastparam");
					}
				}else{
					$(this).addClass("nonparams");
					if(index == 1){
						$(this).children().addClass("desc");
					}else if(index == testparam +1){
						$(this).children().addClass("firstnonparam");
					}
				}
			}else{
				if (index >=2){
					$(this).addClass("params");
					if(index%2 == false){
						$(this).children().addClass("paramstitle");
					}else{
						$(this).children().addClass("paramsdesc");
					}
					if(index==testparam){
						$(this).children().addClass("lastparam");
					}
				}else{
					$(this).addClass("nonparams");
					if(index == 1){
						$(this).children().addClass("desc");
					}
				}
			}		
		});
	});
	
	$("#menuNavigation").height($(document).height());
});

