<script setup>
import LookUpControl from '../components/control/LookUpControl.vue';
import StringControl from '../components/control/StringControl.vue';
import BoolControl from '../components/control/BoolControl.vue';
import NumberControl from '../components/control/NumberControl.vue';
import DateControl from '../components/control/DateControl.vue';
import EntityControl from '../components/control/EntityControl.vue';
import { daxHelper } from '../daxHelper.js'
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'

const selectedEntity = ref(null);
const selectedRecord = ref(null);
const entityDefinition = ref(null);
const attributes = ref([]);
const hiddenAttributes = ['versionnumber', 'utcconversiontimezonecode', 'timezoneruleversionnumber', 'statuscode', 'statecode', 'owninguser', 'owningteam', 'owningbusinessunit', 'ownerid', 'overriddencreatedon', 'modifiedonbehalfby', 'modifiedon', 'modifiedby', 'importsequencenumber', 'createdonbehalfby', 'createdon', 'createdby'];


function loadData(entityInfo, recordLookUp) {

}

function setAttributes() {
    attributes.value = [];
    if (entityDefinition.value?.Attributes) {
        attributes.value = entityDefinition.value.Attributes.filter((x) => {
            return x.AttributeOf == null && !x.IsPrimaryId && !hiddenAttributes.includes(x.LogicalName);
        })
    }
}

watch(() => selectedEntity.value, (newValue, oldValue) => {
    selectedRecord.value = null;
    entityDefinition.value = daxHelper.getEntityDefinition(selectedEntity.value?.MetadataId);
    setAttributes();
})

watch(() => selectedRecord.value, (newValue, oldValue) => {
    loadData(selectedEntity.value, selectedRecord.value);
})

</script>

<template>
    <div style="display: flex;flex-direction: row;justify-content: center;">
        <EntityControl attName="Entity" :required="true" ref="selectEntity" :disabled="false" v-model="selectedEntity">
        </EntityControl>
        <LookUpControl attName="Record" :logicalName="selectedEntity?.LogicalName" :required="true"
            :disabled="selectedEntity == null" v-model="selectedRecord"></LookUpControl>
    </div>

    <div style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center;" v-if="selectedRecord != null">
        <div v-for="attribute in attributes">
            <div v-if="attribute.type == 'InArgument(x:Boolean)'">
                <BoolControl v-model="actionInputData[attribute.name]" :required="attribute.required"
                    :attName="attribute.name" :disabled="false"></BoolControl>
            </div>
            <div
                v-else-if="attribute.type == 'InArgument(x:Double)' || attribute.type == 'InArgument(mxs:Money)' || attribute.type == 'InArgument(x:Decimal)' || attribute.type == 'InArgument(mxs:OptionSetValue)' || attribute.type == 'InArgument(x:Int32)'">
                <NumberControl v-model="actionInputData[attribute.name]" :required="attribute.required"
                    :attName="attribute.name" :disabled="false">
                </NumberControl>
            </div>
            <div v-else-if="attribute.type == 'InArgument(s:DateTime)'">
                <DateControl v-model="actionInputData[attribute.name]" :required="attribute.required"
                    :attName="attribute.name" :disabled="false"></DateControl>
            </div>
            <div v-else-if="attribute.type == 'InArgument(x:String)'">
                <StringControl v-model="actionInputData[attribute.name]" :required="attribute.required"
                    :attName="attribute.name" :disabled="false"></StringControl>
            </div>
            <div v-else>
                <div>Unsupported Attribute:</div>
                <div>Attribute DisplayName:{{ attribute.DisplayName.UserLocalizedLabel?.Label }}</div>
                <div>Attribute LogicalName:{{ attribute.LogicalName }}</div>
                <div>Attribute AttributeType:{{ attribute.AttributeType }}</div>
                <div>Attribute RequiredLevel:{{ attribute.RequiredLevel.Value }}</div>
            </div>
        </div>
    </div>
</template>
