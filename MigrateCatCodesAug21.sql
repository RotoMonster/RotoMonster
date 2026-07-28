

--step 1
select distinct categoriescode from UserLeaguePlayerTypes
union
select distinct categoriescode from OwnershipPlayers
union
select distinct categoriescode from DraftPlayerTypes

--step 2
update UserLeaguePlayerTypes set CategoriesStringId=(select top 1 id from CategoriesStrings cs where cs.Code=UserLeaguePlayerTypes.CategoriesCode)
update OwnershipPlayers set CategoriesStringId=(select top 1 id from CategoriesStrings cs where cs.Code=OwnershipPlayers.CategoriesCode)

--step 3
update DraftPlayerTypes set CategoriesStringId=(select top 1 id from CategoriesStrings cs where cs.Code=DraftPlayerTypes.CategoriesCode)
