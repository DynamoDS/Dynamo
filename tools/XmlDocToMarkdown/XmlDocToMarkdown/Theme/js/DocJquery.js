$('.docutils').each(function(){
	var testparam = $(this).find("td:contains('TypeParam :')").first().parent().prev().index();
	$(this).find('tbody').children().each(function(index){
		if (index >=2 && index<=testparam){
			console.log(index);
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
	});
});